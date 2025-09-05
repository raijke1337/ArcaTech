using UnityEngine;
namespace Arcatech.Texts
{
    [CreateAssetMenu(fileName = "New Extended Description", menuName = "Game/Description/Extended")]
    public class ExtendedText : SimpleText
    {
        public Sprite Picture;
        public string FlavorText;
    }
}