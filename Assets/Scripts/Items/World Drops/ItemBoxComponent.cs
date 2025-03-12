using Arcatech.Actions;
using Arcatech.Level;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    public class ItemBoxComponent : ConditionControlledItem
    {
        [SerializeField] ItemSOContainerComponent DroppedPrefabContainer;
        [SerializeField] ItemSO Content;
        [SerializeField] SerializedActionResult[] Result;

        private void OnValidate()
        {
            Assert.IsNotNull(DroppedPrefabContainer);
            Assert.IsNotNull(Content);
        }
        protected override void OnSetState(bool newstate)
        {
            if (newstate)
            {
                var cont =  Instantiate(DroppedPrefabContainer, transform.position,transform.rotation);                
                cont.Content = Content;
            }
        }
    }
    
}