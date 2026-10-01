using System;
using UnityEngine;

namespace Arcatech.Items.Projectiles
{
    /// <summary>
    /// Правила поражения целей для ОДНОГО снаряда. Живут в конфиге снаряда, а не в применении:
    /// "2 попадания на каждый снаряд" (Nyail Gun), "1 попадание" (орбитальные диски)
    /// и т.п. - свойство снаряда, а не выстрела.
    /// </summary>
    [Serializable]
    public struct ProjectileHitRules
    {
        [Min(0), Tooltip("Сколько попаданий по целям может нанести снаряд. 0 = без ограничения.")]
        public int maxTargets;

        [Tooltip("Разрешить снаряду бить одну и ту же цель повторно (по умолчанию - одна цель один раз).")]
        public bool allowRepeatHits;
    }
}
