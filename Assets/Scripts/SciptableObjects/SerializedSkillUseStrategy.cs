using Arcatech.Actions;
using Arcatech.Items;
using Arcatech.Triggers;
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
        [SerializeField] SerializedUnitAction SkillAction;

        public virtual SkillUsageStrategy ProduceStrategy(BaseEntity owner,SerializedSkill cfg, BaseItemComponent item)
        {
            return new SkillUsageStrategy(item, SkillAction,owner,cfg.Description,Charges,ChargeReload);
        }

        private void OnValidate()
        {
            Assert.IsNotNull(SkillAction);
            Assert.IsFalse(Charges==0);
        }
    }



}