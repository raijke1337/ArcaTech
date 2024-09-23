
using Arcatech.Effects;
using Arcatech.EventBus;
using Arcatech.Skills;
using Arcatech.Texts;
using Arcatech.Triggers;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using CartoonFX;
using System;
using UnityEngine;
namespace Arcatech
{

    #region delegates
    public delegate void SimpleEventsHandler();
    public delegate void SimpleEventsHandler<T>(T arg);
    public delegate void SimpleEventsHandler<T1, T2>(T1 arg1, T2 arg2);



    public delegate void WeaponEvents<T>(T arg);
    public delegate void DodgeEvents<T>(T arg);
    public delegate void SkillEvents<T>(T arg);


    public delegate void ConditionCheckEvents (CheckConditionTrigger condition, bool currentState);

    #endregion

    #region structs 


    [Serializable]
    public class ItemEmpties
    {
        public SerializedDictionary<ItemPlaceType, Transform> ItemPositions;
    }
    public enum ItemPlaceType : byte
    {
        MeleeEmpty,
        RangedEmpty,
        SheathedEmpty,
        ShieldEmpty, 
        BoosterEmpty, 
        Hidden,
        OtherEmpty = 255
    }

    #endregion

    #region puzzle stuff
    [Serializable]
    public class Match2Settings
    {
        [SerializeField, Range(1, 6)] public int Pairs;
        [SerializeField, Range(10, 60)] public float TimeToSolve;
        [SerializeField, Range(0.1f, 5)] public float TimeToShow;
    }
    public struct Pair<T>
    {
        public Pair(T i1, T i2)
        {
            Item1 = i1; Item2 = i2;
        }
        public T Item1 { get; }
        public T Item2 { get; }

        public bool Matching(T i1, T i2)
        {
            return (Item1.Equals(i1) && Item2.Equals(i2)) || (Item1.Equals(i2) && Item2.Equals(i1));
        }
        public bool Contains(T checking)
        {
            return (Item1.Equals(checking) || Item2.Equals(checking));
        }
    }

    #endregion





}
#region AI interfaces  TODO

namespace Arcatech.AI
{ 
    public interface ITargetPoint
    {
        public float AssessDistanceTo(Vector3 position);

    }
    public interface ITargetUnit
    { 
        public bool IsEnemy { get; }
        public float GetHealthPercent { get; }
        public bool IsApproaching { get; }
    
    }



}

#endregion

namespace Arcatech.Effects
{
    public struct VFXRequest : IEvent
    {
        public VFXRequest(CFXR_Effect effect, Transform place,Transform parent = null)
        {
            Effect = effect;
            Parent = parent;
            Place = place;
        }

        public CFXR_Effect Effect { get; }
        public Transform Parent { get; }
        public Transform Place { get; }
    }
}