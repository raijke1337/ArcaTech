namespace Arcatech.UI
{
    public enum OverchargeModuleState
    {
        Idle,
        Ready,
        InSpendWindow,
        Activation,
        Active
    }
    
    public readonly struct OverchargeUISnapshot
    {
        public readonly float CurrentEnergy;
        public readonly float MaxEnergy;

        public readonly float Threshold;
    
        public readonly float WindowSpentEnergy;
        public readonly float RequiredSpentEnergy;

        public readonly float WindowTimeRemaining;
        public readonly float WindowDuration;

        public readonly OverchargeModuleState CurrentState;

        public OverchargeUISnapshot(
            float currentEnergy,
            float maxEnergy,
            float threshold,
            float windowSpentEnergy,
            float requiredSpentEnergy,
            float windowTimeRemaining,
            float windowDuration,
            OverchargeModuleState currentState)
        {
            CurrentEnergy = currentEnergy;
            MaxEnergy = maxEnergy;
            Threshold = threshold;
            WindowSpentEnergy = windowSpentEnergy;
            RequiredSpentEnergy = requiredSpentEnergy;
            WindowTimeRemaining = windowTimeRemaining;
            WindowDuration = windowDuration;
            CurrentState = currentState;
        }
    }
}