using System.Collections;
using UnityEngine;
namespace Arcatech.Managers
{
    public class HitstopPlayer : GenericLazySingleton<HitstopPlayer>
    {
        private Coroutine _routine;
        private float _savedTimeScale = 1f;
        private float _savedFixedDt;

        public void PlayHitstop(float realtime, float speedFraction)
        {
            // Перезаписываем предыдущий hitstop, если ещё идёт
            if (_routine != null)
                StopCoroutine(_routine);
            else
            {
                // Сохраняем "чистое" значение только если сейчас не внутри другого hitstop —
                // иначе при перезаписи мы бы сохранили уже уменьшенный timeScale.
                _savedTimeScale = Time.timeScale;
                _savedFixedDt = Time.fixedDeltaTime;
            }

            _routine = StartCoroutine(Routine(realtime, speedFraction));
        }

        /// <summary>
        /// Немедленно прерывает текущий hitstop и восстанавливает исходный timeScale.
        /// Нужно, когда событие, вызвавшее hitstop, завершилось раньше таймера
        /// (например, игрок успешно нажал кнопку в QTE до истечения slowmo).
        /// </summary>
        public void StopHitstop()
        {
            if (_routine == null) return;

            StopCoroutine(_routine);
            _routine = null;

            Time.timeScale = _savedTimeScale;
            Time.fixedDeltaTime = _savedFixedDt;
        }

        private IEnumerator Routine(float realtime, float speedFraction)
        {
            Time.timeScale      = speedFraction;
            Time.fixedDeltaTime  = _savedFixedDt * speedFraction;

            yield return new WaitForSecondsRealtime(realtime);

            Time.timeScale      = _savedTimeScale;
            Time.fixedDeltaTime  = _savedFixedDt;
            _routine = null;
        }
    }
}