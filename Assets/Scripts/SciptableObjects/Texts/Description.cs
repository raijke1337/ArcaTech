using UnityEngine;
namespace Arcatech.Texts
{

    [CreateAssetMenu(fileName = "New Simple Description", menuName = "Game/Description/Simple")]
    public class Description : ScriptableObject
    {
        public string Title;
        public string Text;
        public Sprite Picture;
        public string FlavorText;
    }
}