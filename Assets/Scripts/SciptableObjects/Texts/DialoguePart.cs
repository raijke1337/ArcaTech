using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Arcatech.Texts
{
    [CreateAssetMenu(fileName = "dialoguePart_", menuName = "Game/Dialogues/Dialogue part")]
    public class DialoguePart : ScriptableObject
    {
        public bool IsForcedDialogue = false;
        public DialogueCharacter Character;
        [SerializeField] string[] DialogueContents;
        public DialoguePart NextDialogue;

        private void OnValidate()
        {
            Assert.IsNotNull(DialogueContents);
        }

        public string Dialogue =>  DialogueContents[Random.Range(0, DialogueContents.Length)];
    }
}