using KBCore.Refs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.Interactions
{
    public class LightChangesColorInteractionHandler : InteractionHandlerBase
    {
        [SerializeField,Self] Light _light;

        [SerializeField, Tooltip("The color of the light on activation")]
        private Color _color = Color.green;
        

        public override void DoInteraction(IInteractor interactor, IInteractive item)
        {
            _light.color = _color;  
        }
    }
}