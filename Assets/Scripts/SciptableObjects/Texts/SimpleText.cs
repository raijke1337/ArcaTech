using UnityEngine;
namespace Arcatech.Texts
{

    [CreateAssetMenu(fileName = "New Simple Description", menuName = "Game/Description/Simple")]
    public class SimpleText : ScriptableObject
    {
        public string Title;
        public string Text;

        public override string ToString()
        {
            return Title+": "+Text;
        }
    }
}