using Arcatech.Units;

namespace Arcatech.Items
{
    public class RangedWeaponStrategy : WeaponStrategy
    {
        public RangedWeaponStrategy(SerializedUnitState act,BaseGameEntityComponent unit, WeaponSO cfg, int charges, float reload, float intcd, BaseEquipmentComponent comp) : base(act, unit, cfg, charges, reload, intcd, comp)
        {
        }

        //// shooting done via extended serialized produce projectiles now
    }
}