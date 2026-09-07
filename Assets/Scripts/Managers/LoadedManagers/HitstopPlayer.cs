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

            _savedTimeScale  = Time.timeScale;
            _savedFixedDt    = Time.fixedDeltaTime;

            _routine = StartCoroutine(Routine(realtime, speedFraction));
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