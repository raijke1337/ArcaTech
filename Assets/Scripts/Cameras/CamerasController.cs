using System;
using System.Collections;
using Arcatech.Managers;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.Cameras
{
    public class CamerasController : GenericLazySingleton<CamerasController>
    {
        [Serializable]
        private struct CameraView
        {
            [Tooltip("Угол вокруг персонажа в градусах. 0, 90, 180, 270 и т. д.")]
            [Range(0f, 360f)]
            public float horizontalAngle;

            [Tooltip("Значение вертикальной оси CinemachineOrbitalFollow.")]
            public float verticalAxisValue;
        }



        [Header("Gameplay camera")]
        [SerializeField] private CinemachineCamera gameplayCamera;
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
        [SerializeField] private CinemachineBrain brain;

        [Header("Preset views")]
        [SerializeField] private CameraView[] views =
        {
            new CameraView { horizontalAngle = 0f, verticalAxisValue = 10f },
            new CameraView { horizontalAngle = 90f, verticalAxisValue = 10f },
            new CameraView { horizontalAngle = 180f, verticalAxisValue = 10f },
            new CameraView { horizontalAngle = 270f, verticalAxisValue = 10f }
        };

        [Header("Rotation")]
        [SerializeField, Min(0.01f)] private float rotationDuration = 0.35f;
        [SerializeField] private bool clockwiseDirectionIsPositive = true;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private int currentViewIndex;
        private Coroutine rotationRoutine;
        private int rotationVersion;

        private void Awake()
        {
            if (gameplayCamera == null)
                gameplayCamera = GetComponent<CinemachineCamera>();

            if (orbitalFollow == null && gameplayCamera != null)
                orbitalFollow = gameplayCamera.GetComponent<CinemachineOrbitalFollow>();

            if (brain == null)
                brain = FindFirstObjectByType<CinemachineBrain>();
        }

        private void Start()
        {
            if (!ValidateReferences())
                return;

            currentViewIndex = FindClosestViewIndex();
            ApplyViewImmediately(currentViewIndex);

            Log(
                $"Initialized. Current view: {currentViewIndex}, " +
                $"angle: {views[currentViewIndex].horizontalAngle}.");
        }

        public void SwitchCamera(bool clockwise, UnityAction onComplete = null)
        {
            if (!ValidateReferences())
                return;

            if (views == null || views.Length == 0)
            {
                LogError("Switch cancelled: preset views array is empty.");
                return;
            }

            int direction = clockwise ? 1 : -1;

            // Если визуально камера едет в противоположную сторону,
            // просто включите этот флаг в Inspector.
            if (!clockwiseDirectionIsPositive)
                direction *= -1;

            int nextViewIndex = Mod(currentViewIndex + direction, views.Length);

            Log(
                $"Switch requested. Clockwise: {clockwise}. " +
                $"Current view: {currentViewIndex}. Next view: {nextViewIndex}.");

            currentViewIndex = nextViewIndex;

            rotationVersion++;

            if (rotationRoutine != null)
            {
                Log(
                    $"Previous rotation cancelled. " +
                    $"Its callback will not be invoked.");

                StopCoroutine(rotationRoutine);
                rotationRoutine = null;
            }

            rotationRoutine = StartCoroutine(
                RotateToView(
                    views[currentViewIndex],
                    direction,
                    rotationVersion,
                    onComplete));
        }

        public void SetView(int viewIndex, UnityAction onComplete = null)
        {
            if (!ValidateReferences())
                return;

            if (views == null || views.Length == 0)
            {
                LogError("SetView cancelled: preset views array is empty.");
                return;
            }

            if (viewIndex < 0 || viewIndex >= views.Length)
            {
                LogError(
                    $"SetView cancelled: index {viewIndex} is outside " +
                    $"the valid range 0..{views.Length - 1}.");

                return;
            }

            if (viewIndex == currentViewIndex)
            {
                Log("SetView: requested view is already active.");
                InvokeCallbackSafely(onComplete);
                return;
            }

            float currentAngle = orbitalFollow.HorizontalAxis.Value;
            float targetAngle = views[viewIndex].horizontalAngle;

            int direction = Mathf.DeltaAngle(currentAngle, targetAngle) >= 0f
                ? 1
                : -1;

            currentViewIndex = viewIndex;
            rotationVersion++;

            if (rotationRoutine != null)
                StopCoroutine(rotationRoutine);

            rotationRoutine = StartCoroutine(
                RotateToView(
                    views[currentViewIndex],
                    direction,
                    rotationVersion,
                    onComplete));
        }

        private IEnumerator RotateToView(
            CameraView targetView,
            int direction,
            int expectedVersion,
            UnityAction onComplete)
        {
            float startHorizontalAngle = orbitalFollow.HorizontalAxis.Value;
            float startVerticalValue = orbitalFollow.VerticalAxis.Value;

            float targetHorizontalAngle = GetDirectedTargetAngle(
                startHorizontalAngle,
                targetView.horizontalAngle,
                direction);

            Log(
                $"Rotation started. " +
                $"Start horizontal: {startHorizontalAngle:F1}; " +
                $"target horizontal: {targetHorizontalAngle:F1}; " +
                $"target vertical: {targetView.verticalAxisValue:F1}.");

            float elapsed = 0f;

            while (elapsed < rotationDuration)
            {
                if (expectedVersion != rotationVersion)
                {
                    Log("Rotation cancelled: a newer switch request was received.");
                    yield break;
                }

                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsed / rotationDuration);

                // Плавность без резкого старта и остановки.
                progress = Mathf.SmoothStep(0f, 1f, progress);

                float horizontalAngle = Mathf.Lerp(
                    startHorizontalAngle,
                    targetHorizontalAngle,
                    progress);

                float verticalValue = Mathf.Lerp(
                    startVerticalValue,
                    targetView.verticalAxisValue,
                    progress);

                SetOrbitAxes(horizontalAngle, verticalValue);

                yield return null;
            }

            SetOrbitAxes(targetHorizontalAngle, targetView.verticalAxisValue);

            // Даём CinemachineBrain обработать последнее изменение осей.
            yield return new WaitForEndOfFrame();

            if (expectedVersion != rotationVersion)
            {
                Log("Callback cancelled: a newer switch request was received.");
                yield break;
            }

            rotationRoutine = null;

            Log(
                $"Rotation completed. " +
                $"Horizontal axis: {orbitalFollow.HorizontalAxis.Value:F1}; " +
                $"Vertical axis: {orbitalFollow.VerticalAxis.Value:F1}.");

            InvokeCallbackSafely(onComplete);
        }

        private void ApplyViewImmediately(int viewIndex)
        {
            CameraView view = views[viewIndex];

            SetOrbitAxes(view.horizontalAngle, view.verticalAxisValue);
        }

        private void SetOrbitAxes(float horizontalAngle, float verticalAxisValue)
        {
            // При включённом Wrap Cinemachine сам приведёт угол к диапазону оси.
            orbitalFollow.HorizontalAxis.Value = horizontalAngle;
            orbitalFollow.VerticalAxis.Value = verticalAxisValue;
        }

        private int FindClosestViewIndex()
        {
            if (views == null || views.Length == 0)
                return 0;

            float currentAngle = orbitalFollow.HorizontalAxis.Value;
            int closestIndex = 0;
            float closestDifference = float.MaxValue;

            for (int i = 0; i < views.Length; i++)
            {
                float difference = Mathf.Abs(
                    Mathf.DeltaAngle(currentAngle, views[i].horizontalAngle));

                if (difference >= closestDifference)
                    continue;

                closestDifference = difference;
                closestIndex = i;
            }

            return closestIndex;
        }

        private static float GetDirectedTargetAngle(
            float currentAngle,
            float targetAngle,
            int direction)
        {
            float currentNormalized = Mathf.Repeat(currentAngle, 360f);
            float targetNormalized = Mathf.Repeat(targetAngle, 360f);

            if (direction > 0)
            {
                float delta = Mathf.Repeat(
                    targetNormalized - currentNormalized,
                    360f);

                return currentAngle + delta;
            }

            float reverseDelta = Mathf.Repeat(
                currentNormalized - targetNormalized,
                360f);

            return currentAngle - reverseDelta;
        }

        private bool ValidateReferences()
        {
            if (gameplayCamera == null)
            {
                LogError("Gameplay Camera is not assigned.");
                return false;
            }

            if (orbitalFollow == null)
            {
                LogError(
                    $"CinemachineOrbitalFollow was not found on " +
                    $"'{gameplayCamera.name}'.");

                return false;
            }

            return true;
        }

        private void InvokeCallbackSafely(UnityAction callback)
        {
            if (callback == null)
            {
                Log("Rotation callback is null.");
                return;
            }

            try
            {
                Log("Invoking rotation callback.");
                callback.Invoke();
                Log("Rotation callback completed successfully.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private static int Mod(int value, int modulo)
        {
            return (value % modulo + modulo) % modulo;
        }

        private void OnDisable()
        {
            if (rotationRoutine == null)
                return;

            StopCoroutine(rotationRoutine);
            rotationRoutine = null;
        }

        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[{nameof(CamerasController)}] {message}", this);
        }

        private void LogError(string message)
        {
            Debug.LogError($"[{nameof(CamerasController)}] {message}", this);
        }
    }
}