using UnityEngine;

namespace Arcatech.Usables
{
    [CreateAssetMenu(fileName = "charges_", menuName = "Usables/Charges/Reload ammo",order = 1)]
    public class SerializedReloadStrategy : SerializedChargesStrategy
    {
        public int usesToReload = 3;
        public float reloadTime = 3f;
        public override BasicChargesStrategy Deserialize()
        {
            return new ReloadStrategy(this);
        }
    }

    public class ReloadStrategy : BasicChargesStrategy
    {
        private readonly int _uses;
        private readonly float _reloadTime;

        private int _usesLeft;
        private CountDownTimer _timer;
        public ReloadStrategy(SerializedReloadStrategy charges) : base(charges)
        {
            _uses = charges.usesToReload;
            _reloadTime = charges.reloadTime;
            _timer = new CountDownTimer(charges.reloadTime);
            _usesLeft = _uses;
        }

        protected override bool ReadyCheck()
        {
            return _usesLeft > 0 && base.ReadyCheck();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
            if (_timer.IsRunning)
            {
                _timer.Tick(delta);
                if (_timer.IsReady) _usesLeft =  _uses;
            }
        }
        
        override public void Use()
        {
            _usesLeft = Mathf.Clamp(_usesLeft--, 0, _uses);
            base.Use();
            if (_usesLeft == 0)
            {
                _timer.Start();
            }
        }

        public override float FillValue
        {
            get
            {
                if (_timer.IsRunning) return _timer.Progress;
                return base.FillValue;
            }
        }

        public override string DisplayText => _usesLeft.ToString();
    }
}