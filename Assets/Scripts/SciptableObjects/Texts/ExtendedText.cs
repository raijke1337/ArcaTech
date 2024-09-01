using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Arcatech.Texts
{
    [CreateAssetMenu(fileName = "New Extended Description", menuName = "Game Stuff/Description/Extended")]
    public class ExtendedText : SimpleText
    {
        public Sprite Picture;
        public string FlavorText;
    }
}