using Arcatech.Stats;
using Arcatech.Texts;
using Arcatech.UI;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Base (only internal)", order = 0)]
    public class SerializedGenericCooldownStrategy : ScriptableObject
    {
        public float cooldown = 0.1f;

        public virtual BasicChargesStrategy Deserialize()
        {
            return new BasicChargesStrategy(this);
        }
    }

    public class BasicChargesStrategy : IReloadStrategy
    {

        public BasicChargesStrategy(SerializedGenericCooldownStrategy charges)
        {
            Cooldown = charges.cooldown;
            MaxCharges = 1;
            CurrentCooldown = 0;
            CurrentCharges = MaxCharges;
        }

        public virtual void Tick(float delta)
        {
            if (CurrentCooldown > 0) CurrentCooldown -= delta;
        }

        protected virtual bool ReadyCheck() => CurrentCooldown <= 0;
        public bool Ready => ReadyCheck();

        public virtual void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            switch (notifyType)
            {
                case StateMachineNotifyType.Use:
                    CurrentCooldown = Cooldown;
                    break;
            }
        }

        public virtual float Cooldown { get; protected set; }
        public virtual float CurrentCooldown { get; protected set; }
        public virtual int MaxCharges { get; protected set; }
        public virtual int CurrentCharges { get; protected set; }

        /// <summary>
        /// Returns null because Cost is provided by the usable
        /// </summary>
        public (ResourceStatType, int) GetCostDescription { get; }
        public Description Description { get; }
    }

    public interface IReloadStrategy : IUsableComponent, IActionIconContent
    {
        public void Tick(float d);
        public bool Ready { get; }

    }
}