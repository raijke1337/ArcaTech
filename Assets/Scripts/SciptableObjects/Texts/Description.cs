using UnityEngine;
namespace Arcatech.Texts
{

    [CreateAssetMenu(fileName = "New Description", menuName = "Game/Description")]
    public class Description : ScriptableObject
    {
        public string Title;
        public string Text;
        public Sprite Picture;
        public string FlavorText;
    }
}