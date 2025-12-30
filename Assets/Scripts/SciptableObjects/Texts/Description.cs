using UnityEngine;
namespace Arcatech.Texts
{

    [CreateAssetMenu(fileName = "description_", menuName = "Game/Description")]
    public class Description : ScriptableObject
    {
        public string Title;
        public string Text;
        public Sprite Picture;
        public string FlavorText;
    }
}