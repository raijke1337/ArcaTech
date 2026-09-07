using System.Collections.Generic;
using System.Linq;
using Arcatech.Audio;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "actionResult_PlaySound_", menuName = "Actions/Action Result/FX/Random Sound")]
    public class SerializedProduceSoundResult : SerializedActionResult
    {
        [Header("int is random weight")]
        [SerializeField] SerializedDictionary<SoundDefinition, int> soundDefinitions;

        public override ActionResult Deserialize()
        {
            return new ProduceSoundResult(soundDefinitions);
        }
    }

    public class ProduceSoundResult : ActionResult
    {
        private Dictionary<SoundDefinition, int> dict;

        public ProduceSoundResult(Dictionary<SoundDefinition, int> soundDefinitions) => dict = soundDefinitions;

        public override bool ProduceResult(BaseGameEntityComponent user, BaseGameEntityComponent target, Vector3 place, Quaternion placeRot)
        {
            var sound = GetSound;
            if (sound != null)
            {
                AudioEvents.Play(sound, place);
            }
            return true;
        }

        SoundDefinition GetSound 
        {
            get
            {
                if (dict == null || dict.Count == 0)
                    return null;

                // 1. Подсчитываем общий вес
                int totalWeight = 0;
                foreach (var pair in dict)
                {
                    if (pair.Key != null && pair.Value > 0)
                    {
                        totalWeight += pair.Value;
                    }
                }

                // 2. Если все веса нулевые или неположительные, выбираем любой случайный
                if (totalWeight <= 0)
                {
                    var keysArray = dict.Keys.ToArray();
                    return keysArray[Random.Range(0, keysArray.Length)];
                }

                // 3. Взвешенный случайный выбор
                int randomValue = Random.Range(0, totalWeight);
                foreach (var pair in dict)
                {
                    if (pair.Key == null || pair.Value <= 0)
                        continue;

                    if (randomValue < pair.Value)
                    {
                        return pair.Key;
                    }

                    randomValue -= pair.Value;
                }

                // Fallback на случай погрешностей
                return dict.Keys.FirstOrDefault(k => k != null);
            }
        }
    }
}