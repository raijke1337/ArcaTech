using Arcatech.Effects;
using Arcatech.Texts;
using UnityEngine;

namespace Arcatech.Scenes
{
    [CreateAssetMenu(fileName = "New level", menuName = "Game/Level")]
    public class SceneContainer : ScriptableObjectID
    {
        public int SceneLoaderIndex;
        public LevelType LevelType;
        public Description Description;
        public SoundClipData Music;
        public bool IsUnlockedByDefault;
        public SceneContainer NextLevel;
    }
}