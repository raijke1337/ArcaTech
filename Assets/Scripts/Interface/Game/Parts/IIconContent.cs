using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.UI
{
    public interface IIconContent
    {
        public Description Description { get; }
        public float FillValue { get; }
        public string IconNumber { get; }
    }
}