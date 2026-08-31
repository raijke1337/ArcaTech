using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Units
{
    [RequireComponent(typeof(BaseGameEntityComponent))]
    public class PlayerComponent : ValidatedMonoBehaviour, ITierProvider
    {
        
        [SerializeField,Self]public BaseGameEntityComponent Entity;
        
        public UnitTier GetTierInfo => UnitTier.Boss;
    }
}