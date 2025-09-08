using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Interactions
{
    /// <summary>
    /// this is a class that indicated a mesh that will glow up when moused over
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class GlowOutlinePickerComponent : ValidatedMonoBehaviour
    {
        [SerializeField, Self] private Renderer _r;
        public Renderer GetRenderer => _r;

    }
}