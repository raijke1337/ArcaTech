using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Reload ammo",order = 1)]
    public class SerializedReloadStrategy : SerializedGenericCooldownStrategy
    {
        [Min(1)] public int maxCharges = 3;
        [Min(0f)]public float reloadTime = 3f;
        public override BasicChargesStrategy Deserialize()
        {
            return new ReloadStrategy(this);
        }
    }

    public class ReloadStrategy : BasicChargesStrategy
    {
        public ReloadStrategy(SerializedReloadStrategy charges) : base(charges)
        {
            MaxCharges =  charges.maxCharges;
            CurrentCharges = MaxCharges;
            Cooldown = charges.reloadTime;
            CurrentCooldown = 0;
        }

        protected override bool ReadyCheck()
        {
            return CurrentCharges > 0;
        }

        public override void Tick(float delta)
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown -= delta;
                if (CurrentCooldown <= 0)
                {
                    CurrentCharges = MaxCharges;
                }
            }
        }

        public override void OnChangeUsableState(StateMachineNotifyType notifyType)
        {
            base.OnChangeUsableState(notifyType);
            switch (notifyType)
            {
                case StateMachineNotifyType.Use:
                {
                    CurrentCharges -= 1;
                    if (CurrentCharges <= 0)
                    {
                        CurrentCooldown = Cooldown;
                    }
                    break;
                }
            }
        }
    }
}