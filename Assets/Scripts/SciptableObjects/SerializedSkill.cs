using Arcatech.Items;
using Arcatech.Texts;
using Arcatech.Triggers;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Skills
{
    [CreateAssetMenu(fileName = "New Skill Config", menuName = "Items/Skills/Skill")]
    public class SerializedSkill : ScriptableObject
    {
        [Header("Use strategy"),SerializeField] public SerializedSkillUseStrategy UseStrategy;
        [SerializeField] DrawItemsStrategy DrawItemsStrategy;
        [Header("Text"), SerializeField] public ExtendedText Description;

        [Space, Header("Combat")]
        //public UnitActionType UnitActionType;
        [SerializeField] public SerializedStatsEffectConfig Cost;


        public Skill CreateSkill(BaseGameEntityComponent owner, BaseItemComponent item,EquipmentType type)
        {
            return new Skill(DrawItemsStrategy, this,owner,item,type);
        }
        private void OnValidate()
        {
            Assert.IsNotNull(UseStrategy);
            Assert.IsNotNull(Cost);
            Assert.IsNotNull(DrawItemsStrategy);
            Assert.IsNotNull(Description);
        }
    }
}