using System;
using System.Collections.Generic;
using CartoonFX;
using DG.Tweening;
using UnityEngine;

namespace Arcatech.Interactions
{
    public class ParticlesActivatorHandler : InteractionHandlerBase
    {
        private List<CFXR_Effect> _particles;
        [SerializeField, ReadOnlyText] private string info = "";
        private void Start()
        {
            _particles = new List<CFXR_Effect>();
            _particles.AddRange(GetComponentsInChildren<CFXR_Effect>());
            info = $"Detected {_particles.Count} particles.";
            foreach (var particle in _particles)
            {
                particle.gameObject.SetActive(false);
            }
        }

        public override void DoInteraction(bool success, IInteractor interactor)
        {
            if (success)
            {
                foreach (var p in _particles)
                {
                    p.gameObject.SetActive(true);
                }
            }
        }

        public override void OnPlayerEnter()
        {
        }

        public override void OnPlayerExit()
        {
            foreach (var p in _particles)
            {
                p.gameObject.transform.DOScale(Vector3.zero, 0.5f).onComplete += () => p.gameObject.SetActive(false);
            }
        }
    }
}