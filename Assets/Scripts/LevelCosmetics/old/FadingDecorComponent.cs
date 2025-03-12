using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Arcatech.Level
{
    public class FadingDecorComponent : MonoBehaviour//, IEquatable<FadingDecorComponent>
    {

        [SerializeField] SerializedDictionary<Renderer, Material[]> _original = new();
        [HideInInspector] public float InitialAlpha;

        private void Awake()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                var mats = r.materials;
                _original[r] = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    _original[r][i] = mats[i];
                }
            }
        }

        public void Fade(float speed, Material newMaterial)
        {
            foreach(var r in _original.Keys)
            {
                r.material = newMaterial;
            }
        }
        public void Unfade()
        {
            foreach (var r in _original.Keys)
            {
                r.material = _original[r][0];
            }
        }
        public override int GetHashCode()
        {
            return transform.GetHashCode();
        }

    }
}