using Arcatech.Items;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Skills
{
    [CreateAssetMenu(fileName = "New Skill use Logic", menuName = "Items/Skills/SkillUseLogic")]
    public class SerializedSkillUseStrategy : ScriptableObject
    {
        [Header("Cooldowns")]
        [SerializeField] int Charges;
        [SerializeField] float ChargeReload;

        [Space,Header("Usage")]
        [SerializeField] SerializedUnitState skillState;

        public virtual SkillUsageStrategy ProduceStrategy(ActiveGameUnitComponent owner,SerializedSkill cfg, EquipmentComponent item)
        {
            return new SkillUsageStrategy(item, skillState,owner,cfg.Description,Charges,ChargeReload);
        }

        private void OnValidate()
        {
            Assert.IsNotNull(skillState);
            Assert.IsFalse(Charges==0);
        }
    }



}