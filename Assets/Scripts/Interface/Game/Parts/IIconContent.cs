using UnityEngine;

namespace Arcatech.UI
{
    public interface IIconContent
    {
        public Sprite Icon { get; }
        public float FillValue { get; }
        public string IconValue { get; }

    }
}