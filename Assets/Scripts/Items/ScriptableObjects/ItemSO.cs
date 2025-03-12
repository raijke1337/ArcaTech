using Arcatech.Texts;
using com.cyborgAssets.inspectorButtonPro;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using System.Reflection;
using UnityEditor.Compilation;
using Unity.Properties;

namespace Arcatech.Items
{
    [Serializable, CreateAssetMenu(fileName = "New Backpack Item", menuName = "Items/Just Item")]
    public class ItemSO : ScriptableObjectID
    {
        public BaseItemComponent ItemPrefab;
        public ExtendedText Description;        
        public EquipmentType Type;

        [Space] public int MaxStack = 1;
        protected virtual void OnValidate()
        {
            Assert.IsFalse(Type==EquipmentType.None);
        }


        //[ProButton]
        //protected void CheckAttributes()
        //{
        //    var type = this.GetType();
        //    var fields = type.GetFields();

        //    foreach (var field in fields)
        //    {
        //        Debug.Log($"public field {field.Name} type is {field.FieldType}, base is SO: {field.FieldType.BaseType.IsAssignableFrom(typeof(ScriptableObject))}");
        //    }
        //}
    }
}