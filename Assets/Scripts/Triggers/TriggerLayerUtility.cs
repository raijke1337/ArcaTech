using Arcatech.Managers;
using UnityEngine;

namespace Arcatech.Triggers
{
    /// <summary>
    /// Общая логика определения "хитового" слоя (Valid/Invalid, см.
    /// DataManager.GameRules.ValidHitsLayer / InvalidHitsLayer). Раньше маски
    /// вычислялись и хранились отдельно внутри TriggerTrackerComponent — теперь
    /// это единая точка правды, чтобы любой источник попаданий (обычный триггер,
    /// AreaCast, лучевое оружие вроде BeamWeaponComponent и т.п.) классифицировал
    /// хиты одинаково, без дублирования битовой арифметики и риска, что где-то
    /// маски посчитаются иначе.
    /// </summary>
    public static class TriggerLayerUtility
    {
        private static LayerMask? _valid;
        private static LayerMask? _invalid;

        public static LayerMask ValidMask => _valid ??= LayerMask.GetMask(DataManager.GameRules.ValidHitsLayer);

        public static LayerMask InvalidMask => _invalid ??= LayerMask.GetMask(DataManager.GameRules.InvalidHitsLayer);

        /// <summary>
        /// Valid | Invalid — маска, которую должен слушать любой Collider/
        /// OverlapXXX/RaycastAll, если он хочет ловить именно "хитовые" объекты.
        /// </summary>
        public static LayerMask HitMask => ValidMask | InvalidMask;

        public static HitLayerKind Resolve(GameObject target)
        {
            if (target == null) return HitLayerKind.Unknown;

            var layerBit = 1 << target.layer;

            if ((ValidMask.value & layerBit) != 0) return HitLayerKind.Valid;
            if ((InvalidMask.value & layerBit) != 0) return HitLayerKind.Invalid;

            return HitLayerKind.Unknown;
        }

        public static HitLayerKind Resolve(Collider collider) =>
            collider != null ? Resolve(collider.gameObject) : HitLayerKind.Unknown;

        /// <summary>
        /// Сбрасывает закэшированные маски слоёв. Нужно вызвать, если
        /// DataManager.GameRules может смениться в рантайме (например, смена
        /// профиля правил/сложности) — иначе значения посчитаются один раз при
        /// первом обращении и останутся такими до перезапуска.
        /// </summary>
        public static void InvalidateCache()
        {
            _valid = null;
            _invalid = null;
        }
    }
}