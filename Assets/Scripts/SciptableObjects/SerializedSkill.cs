using System.Security.Cryptography;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.Triggers;
using Arcatech.UI;
using Arcatech.Units;
using UnityEngine;
using UnityEngine.Assertions;
namespace Arcatech.Skills
{
    [CreateAssetMenu(fileName = "New Skill Config", menuName = "Items/Skills/Skill")]
    public class SerializedSkill : ScriptableObject, IIconContent
    {
        [Header("Use strategy"),SerializeField] public SerializedSkillUseStrategy UseStrategy;
        [SerializeField] DrawItemsStrategy DrawItemsStrategy;
        [Header("Text"), SerializeField] Description description;
        
        [Space, Header("Combat")]
        [SerializeField] public StatsEffect Cost;
        public SerializedStateTransition StateTransition;

        public Skill CreateSkill(BaseGameEntityComponent owner, EquipmentComponent item,ItemType type)
        {
            return new Skill(StateTransition,DrawItemsStrategy, this,owner,item,type);
        }
        private void OnValidate()
        {
            Assert.IsNotNull(UseStrategy);
            //Assert.IsNotNull(Cost);
            Assert.IsNotNull(DrawItemsStrategy);
            Assert.IsNotNull(Description);
        }

        public Description Description => description;
        public float FillValue => 0;
        public string IconNumber => string.Empty;
    }
}