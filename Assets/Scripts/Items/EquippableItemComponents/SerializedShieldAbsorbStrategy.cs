using Arcatech.Actions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Items
{
    [CreateAssetMenu(fileName = "Shield Absorb Strategy", menuName = "Items/Use strategy/Shield absorb")]
    public class SerializedShieldAbsorbStrategy : ScriptableObject
    {
        [Range(0, 1), Tooltip("Portion of dmg to be absorbed, 1 means all dmg will be converted to energy"), SerializeField] float AbsorbPercent = 0.1f;
        [Range(0, 1), Tooltip("Dmg to energy convert ratio, at 1 1 dmg absorbed equals 1 energy spent"), SerializeField] float AbsorbReduction = 1f;
        [Tooltip("Time over which instant absorb effect is split,0 is instant"),SerializeField,Range(0,10)] int AbsorbTime = 1;
        [Space, SerializeField] SerializedActionResult OnApply;

        private void OnValidate()
        {
            Assert.IsNotNull(OnApply);
        }
        public ShieldAbsorbStrategy BuildStrategy
        {
            get
            {
                return new ShieldAbsorbStrategy(AbsorbPercent, AbsorbReduction, AbsorbTime, OnApply);
            }
        }
    }

}