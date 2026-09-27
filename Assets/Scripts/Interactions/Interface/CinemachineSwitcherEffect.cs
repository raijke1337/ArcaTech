using Unity.Cinemachine;
using UnityEngine;

namespace Arcatech.Interactions
{
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CinemachineSwitcherEffect : InteractionEffect
    {
        CinemachineCamera cam;

        private void Awake()
        {
            cam = GetComponent<CinemachineCamera>();
        }

        public override void Play(InteractionContext ctx)
        {
            if (ctx.State == InteractionState.InProgress) cam.Priority = 2;
            else cam.Priority = -1;
        }
    }
}