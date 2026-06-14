namespace Arcatech.Usables.Effects
{
    public sealed class ActiveEffectInstance
    {
        
        public EffectKey Key { get; }
        public IEffectResult Result { get; }
        public BaseGameEntityComponent Source { get; }   // source kept with the instance
        public BaseAppliedEffect.StackType StackType { get; }
        public int MaxStacks { get; }
        public string EffectId => Key.EffectId;
        
        private readonly PeriodicityRunner _runner;
        private readonly bool _infinite;
        private float _elapsed;
        private int _ticksFired;

        public int Stacks { get; internal set; } = 1;
        public bool IsFinished { get; private set; }

        public ActiveEffectInstance(EffectKey key, IEffectResult result, BaseGameEntityComponent source,
            PeriodicityRunner runner, bool infinite, BaseAppliedEffect.StackType stackType, int MaxStacks)
        {
            Key = key;
            Result = result;
            Source = source;
            _runner = runner;
            _infinite = infinite;
            MaxStacks = MaxStacks;
            StackType = stackType;
        }

        public void Tick(float dt, EffectContext ctx)
        {
            if (IsFinished) return;

            ctx.Source = Source;       
            ctx.Instance = this;
            _elapsed += dt;

            while (_runner.TryConsumeTick(_elapsed))
            {
                ctx.TickIndex = _ticksFired;
                Result.Apply(ctx);
                _ticksFired++;
            }

            if (!_infinite && _elapsed >= _runner.TotalDuration && _runner.AllTicksConsumed)
            {
                Result.OnExpire(ctx);
                IsFinished = true;
            }
        }

        public void ForceExpire(EffectContext ctx)
        {
            if (IsFinished) return;
            ctx.Source = Source;
            ctx.Instance = this;
            Result.OnExpire(ctx);
            IsFinished = true;
        }

        public void RefreshLifetime()
        {
            _elapsed = 0f;
            _ticksFired = 0;
            _runner.Reset();
            IsFinished = false;
        }
    }
}