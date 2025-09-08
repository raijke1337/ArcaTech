using System.Linq;
using UnityEngine;
namespace Arcatech.Texts
{
    [CreateAssetMenu(fileName = "New Extended Description", menuName = "Game/Description/Extended")]
    public class ExtendedText : SimpleText
    {
        public Sprite Picture;
        public string FlavorText;
        public override string ToString()
        {
            var s = base.ToString();
            s += "\n" + FlavorText;
            return s;
        }
    }
}