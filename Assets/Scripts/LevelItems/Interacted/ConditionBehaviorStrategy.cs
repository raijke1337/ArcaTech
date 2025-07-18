namespace Arcatech.Level.Conditions
{
    public abstract class ConditionBehaviorStrategy : ScriptableObjectID
    {
        public abstract IConditionControlledStrat Build(ConditionControlledItemComponent item);
    }

    public abstract class ConditionControlledItemBehaviourStrategy : IConditionControlledStrat
    {
        public abstract void SetState(ConditionCheckResult newstate);
    }
    public interface IConditionControlledStrat : IStrategy, IConditionControlled { }

    public enum ConditionCheckResult
    {
        None,
        Success,
        Fail
    }
}