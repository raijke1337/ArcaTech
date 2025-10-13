using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Texts
{
    [CreateAssetMenu(fileName = "New Dialogue part part", menuName = "Game/Dialogues/Dialogue")]
    public class DialoguePart : ScriptableObject
    {
        public DialogueCharacter Character;

    //   public FaceExpression Mood;
        public Description DialogueContent;

        public SerializedDictionary<Description, DialoguePart> Options;

    }
}