using System;
using Unity.Collections;
using UnityEngine;

namespace Arcatech.Lewding
{
    public class LewdnessContext
    {

        private float _arousal;

        public float ArousalPercent
        {
            get => _arousal;
            set
            {
                _arousal = value;
                if (_arousal >= _settings.stageOnePercent)
                {
                    LewdStage = 1;
                }

                if (_arousal >= _settings.stageTwoPercent)
                {
                    LewdStage = 2;
                }
                _animator.SetFloat(_paramIndex,LewdStage);
            }
        }

        public int LewdStage { get; private set; }
        public TouchZoneType LastTouchCommand { get; set; }
        private readonly LewdnessSettings _settings;
        private readonly Animator _animator;

        [SerializeField] private const string AnimatorLewdnessParameter = "LewdnessStage";
        private readonly int _paramIndex;

        public bool InitializeAnimationFlag { get; set; }

        public LewdnessContext(LewdnessSettings cfg,Animator animator)
        {
            _paramIndex = Animator.StringToHash(AnimatorLewdnessParameter);
            _settings = cfg;
            _animator =  animator;
            LewdStage = 0;
            _animator.SetFloat(_paramIndex,0);
        }
        
        
    }
}