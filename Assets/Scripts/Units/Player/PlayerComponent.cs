using UnityEngine;

namespace Arcatech.Units
{
    public class PlayerComponent : MonoBehaviour, ITierProvider
    {
        public UnitTier GetTierInfo => UnitTier.Boss;
    }
}