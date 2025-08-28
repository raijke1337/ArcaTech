
namespace Arcatech.Items
{
    public class Itemfactory
    {

        public static Itemfactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Itemfactory();
                }
                return _instance;
            }
        }
        static Itemfactory _instance;

        public IItem ProduceItem(ItemSO cfg, BaseGameEntityComponent owner)
        {
            return cfg.Type switch
            {
                EquipmentType.MeleeWeap => new Weapon(cfg as WeaponSO, owner),
                EquipmentType.RangedWeap => new Weapon(cfg as WeaponSO, owner),
                EquipmentType.Shield => new Shield(cfg as ShieldSO, owner),
                EquipmentType.Booster => new Equipment(cfg as EquipSO, owner),
                _ => new Item(cfg, owner),
            };
        }
    }
}
