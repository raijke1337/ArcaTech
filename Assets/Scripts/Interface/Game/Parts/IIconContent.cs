using UnityEngine;
using UnityEngine.Events;

namespace Arcatech.UI
{
    public interface IIconContent
    {
        public Sprite Icon { get; }
        public float FillValue { get; }
        public string IconValue { get; }

    }
}