using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Проверяет видимость центра и точек в диске вокруг Tracking Target.
/// Если стеной закрыта хотя бы одна точка, добавляет камере подъём по мировой оси Y.
/// Ротация и параметры ортографической линзы не меняются.
/// </summary>
[AddComponentMenu("Cinemachine/Extensions/Raise For Area Occlusion")]
public class CameraDeocclusion : CinemachineExtension
{
    [Header("Area around target")] [Min(0f)]
    public float radius = 3f;

    [Range(4, 32)] public int outerRingSamples = 12;

    [Range(0, 32)] public int innerRingSamples = 8;

    [Range(0f, 1f)] public float innerRingRadiusFraction = 0.5f;

    [Header("Obstacles")] public LayerMask obstacleLayers;

    public QueryTriggerInteraction triggerInteraction =
        QueryTriggerInteraction.Ignore;

    [Header("Vertical avoidance")] [Min(0f)]
    public float maxRaise = 12f;

    [Min(0.05f)] public float raiseStep = 0.5f;

    [Tooltip("Время плавного подъёма камеры при появлении препятствия.")] [Min(0f)]
    public float raiseDamping = 0.15f;

    [Tooltip("Время плавного возврата камеры вниз.")] [Min(0f)]
    public float returnDamping = 0.35f;

    class ExtraState : VcamExtraStateBase
    {
        public float currentRaise;
        public float velocity;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
            return;

        if (vcam.Follow == null || radius <= 0f)
            return;

        var extra = GetExtraState<ExtraState>(vcam);

        Vector3 targetPosition = vcam.Follow.position;
        Vector3 baseCameraPosition = state.GetFinalPosition();

        // Forward направлен ИЗ камеры в сцену.
        Vector3 cameraForward = state.GetFinalOrientation() * Vector3.forward;

        // Backward — от сцены назад, к камере.
        // При наклонённой вниз камере это также направление вверх.
        Vector3 raiseDirection = -cameraForward.normalized;

        // Если камера направлена не вниз, она не сможет подниматься,
        // двигаясь назад по оси своего взгляда.
        if (raiseDirection.y <= 0.001f)
            return;

        float desiredRaise = FindRequiredRaise(
            baseCameraPosition,
            targetPosition,
            raiseDirection);

        if (deltaTime < 0f)
        {
            extra.currentRaise = desiredRaise;
            extra.velocity = 0f;
        }
        else
        {
            float smoothTime = desiredRaise > extra.currentRaise
                ? raiseDamping
                : returnDamping;

            extra.currentRaise = Mathf.SmoothDamp(
                extra.currentRaise,
                desiredRaise,
                ref extra.velocity,
                smoothTime,
                Mathf.Infinity,
                deltaTime);
        }

        // currentRaise измеряется в метрах по мировой высоте Y.
        // Переводим его в расстояние вдоль направления назад от камеры.
        float distanceAlongViewAxis = extra.currentRaise / raiseDirection.y;

        // Смещение строго вдоль оптической оси.
        // Поэтому цель не сдвигается в ортографическом кадре.
        state.PositionCorrection += raiseDirection * distanceAlongViewAxis;
    }

    float FindRequiredRaise(
        Vector3 baseCameraPosition,
        Vector3 targetPosition,
        Vector3 raiseDirection)
    {
        for (float raise = 0f; raise <= maxRaise; raise += raiseStep)
        {
            // raise — желаемый прирост высоты по мировой оси Y.
            float distanceAlongViewAxis = raise / raiseDirection.y;

            Vector3 candidateCameraPosition =
                baseCameraPosition + raiseDirection * distanceAlongViewAxis;

            if (IsWholeAreaVisible(candidateCameraPosition, targetPosition))
                return raise;
        }

        return maxRaise;
    }

    bool IsWholeAreaVisible(Vector3 cameraPosition, Vector3 targetPosition)
    {
        // Центр области.
        if (IsBlocked(cameraPosition, targetPosition))
            return false;

        // Внутреннее кольцо.
        if (!CheckRing(
                cameraPosition,
                targetPosition,
                radius * innerRingRadiusFraction,
                innerRingSamples))
            return false;

        // Граница круга радиуса radius.
        if (!CheckRing(
                cameraPosition,
                targetPosition,
                radius,
                outerRingSamples))
            return false;

        return true;
    }

    bool CheckRing(
        Vector3 cameraPosition,
        Vector3 center,
        float ringRadius,
        int samples)
    {
        if (samples <= 0 || ringRadius <= 0f)
            return true;

        for (int i = 0; i < samples; ++i)
        {
            float angle = i * Mathf.PI * 2f / samples;

            Vector3 point = center + new Vector3(
                Mathf.Cos(angle) * ringRadius,
                0f,
                Mathf.Sin(angle) * ringRadius);

            if (IsBlocked(cameraPosition, point))
                return false;
        }

        return true;
    }

    bool IsBlocked(Vector3 cameraPosition, Vector3 point)
    {
        Vector3 direction = point - cameraPosition;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return false;

        return Physics.Raycast(
            cameraPosition,
            direction / distance,
            distance,
            obstacleLayers,
            triggerInteraction);
    }
}