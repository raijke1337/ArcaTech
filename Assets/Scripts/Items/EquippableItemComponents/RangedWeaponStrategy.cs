using Arcatech.Actions;
using Arcatech.Units;
using System.Collections;
using UnityEngine;

namespace Arcatech.Items
{
    public class RangedWeaponStrategy : WeaponStrategy
    {
        public RangedWeaponStrategy(SerializedUnitAction act,BaseGameEntityComponent unit, WeaponSO cfg, int charges, float reload, float intcd, BaseWeaponComponent comp) : base(act, unit, cfg, charges, reload, intcd, comp)
        {
        }

        //// shooting done via extended serialized produce projectiles now
    }
}