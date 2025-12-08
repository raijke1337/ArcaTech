using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Interactions
{
    public class LightChangesColorInteractionHandler : InteractionHandlerBase
    {
        [SerializeField,Self] Light _light;

        [SerializeField, Tooltip("The color of the light on activation")]
        private Color successColor = Color.green;
        [SerializeField, Tooltip("The color of the light on fail")]
        private Color failureColor = Color.red;

        public override void DoInteraction(bool success, IInteractor interactor)
        {
            _light.color = success? successColor : failureColor;
        }

        public override void OnPlayerEnter()
        {
        }

        public override void OnPlayerExit()
        {
        }
    }
}