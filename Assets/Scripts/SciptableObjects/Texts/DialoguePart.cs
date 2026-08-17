using System;
using Arcatech.Audio;
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
        [SerializeField] DialogueLine[] DialogueContents;
        public DialoguePart NextDialogue;

        private void OnValidate()
        {
            Assert.IsNotNull(DialogueContents);
        }

        public DialogueLine Dialogue 
        {
            get
            {
                if (DialogueContents == null || DialogueContents.Length == 0)
                    return null;

                if (DialogueContents.Length == 1)
                    return DialogueContents[0];

                int totalWeight = 0;
                for (int i = 0; i < DialogueContents.Length; i++)
                {
                    var line = DialogueContents[i];
                    if (line != null && line.Weight > 0)
                    {
                        totalWeight += line.Weight;
                    }
                }

                if (totalWeight <= 0)
                {
                    return DialogueContents[UnityEngine.Random.Range(0, DialogueContents.Length)];
                }

                // 3. Генерируем случайное число от 0 (включительно) до totalWeight (исключительно)
                int randomValue = UnityEngine.Random.Range(0, totalWeight);

                // 4. Находим элемент, соответствующий выпавшему числу
                for (int i = 0; i < DialogueContents.Length; i++)
                {
                    var line = DialogueContents[i];
                    if (line == null || line.Weight <= 0)
                        continue;

                    if (randomValue < line.Weight)
                    {
                        return line;
                    }

                    randomValue -= line.Weight;
                }

                return DialogueContents[0];
            }
        }
    }
    
    [Serializable]
    public class DialogueLine
    {
        public int Weight = 1;
        public string Text;
        public SoundDefinition VoiceLine;

        public bool TryGetVoiceLine(out SoundDefinition voiceLine)
        {
            voiceLine = VoiceLine;
            return voiceLine != null;
        }
    }
}