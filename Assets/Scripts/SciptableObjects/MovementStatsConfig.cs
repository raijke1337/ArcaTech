using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

namespace Arcatech.Units.Stats
{
    [CreateAssetMenu(fileName = "New MoveStatsConfig", menuName = "Units/Move Stats"),Serializable]
    public class MovementStatsConfig : ScriptableObjectID
    {
        public SerializedDictionary<MovementStatType, float> Stats;
        [SerializeField] public SerializedUnitState jumpState;
    }
    public enum MovementStatType
    {
        Movespeed,
        TurnSpeed,
        JumpHeight
    }



}