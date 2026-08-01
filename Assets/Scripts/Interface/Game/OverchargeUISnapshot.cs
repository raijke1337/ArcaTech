namespace Arcatech.UI
{
    public readonly struct OverchargeUISnapshot
    {
        public readonly float CurrentEnergy;
        public readonly float MaxEnergy;

        public readonly float Threshold;
    
        public readonly bool IsWindowActive;
        public readonly float WindowSpentEnergy;
        public readonly float RequiredSpentEnergy;

        public readonly float WindowTimeRemaining;
        public readonly float WindowDuration;

        public readonly bool IsOverchargeReady;
        public readonly bool IsOverchargeActive;

        public OverchargeUISnapshot(
            float currentEnergy,
            float maxEnergy,
            float threshold,
            bool isWindowActive,
            float windowSpentEnergy,
            float requiredSpentEnergy,
            float windowTimeRemaining,
            float windowDuration,
            bool isOverchargeReady,
            bool isOverchargeActive)
        {
            CurrentEnergy = currentEnergy;
            MaxEnergy = maxEnergy;
            Threshold = threshold;
            IsWindowActive = isWindowActive;
            WindowSpentEnergy = windowSpentEnergy;
            RequiredSpentEnergy = requiredSpentEnergy;
            WindowTimeRemaining = windowTimeRemaining;
            WindowDuration = windowDuration;
            IsOverchargeReady = isOverchargeReady;
            IsOverchargeActive = isOverchargeActive;
        }
    }
}