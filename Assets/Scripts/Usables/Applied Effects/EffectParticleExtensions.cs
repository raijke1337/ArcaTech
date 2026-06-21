using DG.Tweening;
using UnityEngine;

public static class EffectParticleExtensions
{
    /// <summary>
    /// Плавно останавливает эмиссию, опционально фейдит альфу материала,
    /// затем уничтожает GameObject. Длительность подбирается по max lifetime.
    /// </summary>
    public static void FadeOutAndDestroy(this ParticleSystem ps, float maxWait = 2f)
    {
        if (ps == null) return;

        var go = ps.gameObject;

        // 1) Прекратить рождение новых частиц, но НЕ убивать уже живущие
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // 2) Сколько ждать — суммируем максимальный lifetime
        //    корневого и всех дочерних ParticleSystem'ов
        float maxLifetime = GetMaxLifetime(ps);
        float wait = Mathf.Min(maxLifetime + 0.25f, maxWait);

        // 3) (опционально) Фейдим альфу материала, чтобы фейд был заметным
        //    даже если Color over Lifetime в ParticleSystem не настроен.
        var rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            // .material создаёт инстанс — оригинальный шейдер на префабе не трогается
            var mat = rend.material;
            mat.DOFade(0f, wait)
               .SetEase(Ease.OutQuad);
        }

        // 4) Уничтожение через глобальный таймер DOTween —
        //    НЕ привязан к GameObject, поэтому не «умирает» вместе с ним.
        DOVirtual.DelayedCall(wait, () =>
        {
            if (go != null) Object.Destroy(go);
        });
    }

    private static float GetMaxLifetime(ParticleSystem root)
    {
        float max = SafeLifetime(root.main.startLifetime);
        var children = root.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var p in children)
        {
            float l = SafeLifetime(p.main.startLifetime);
            if (l > max) max = l;
        }
        return max;
    }

    private static float SafeLifetime(ParticleSystem.MinMaxCurve curve)
    {
        // Если mode == TwoConstants — берём constantMax, иначе constant
        return curve.mode == ParticleSystemCurveMode.TwoConstants
            ? curve.constantMax
            : curve.constant;
    }
}