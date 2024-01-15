
using Arcatech.Effects;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.UI;
using Arcatech.Units;
using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
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

    public delegate void StateMachineEvent();
    public delegate void StateMachineEvent<T>(T arg);


    public delegate void SkillRequestedEvent(SkillComponent data, BaseUnit source, Transform where);
    public delegate void EffectsManagerEvent(EffectRequestPackage effectRequestPackage);

    public delegate void SimpleTriggerEvent(BaseUnit target, bool isEnter);
    public delegate void TriggerEvent(BaseUnit target, BaseUnit source, bool isEnter, BaseStatTriggerConfig cfg);
    #endregion

    #region structs 

    #region puzzle stuff
    [Serializable]
    public class Match2Settings
    {
        [SerializeField,Range(1,6)]public int Pairs;
        [SerializeField, Range(10, 60)] public float TimeToSolve;
        [SerializeField, Range(0.1f, 5)] public float TimeToShow;
    }
    public struct Pair<T>
    {
        public Pair (T i1, T i2)
        {
            Item1 = i1; Item2 = i2;
        }
        public T Item1 { get; }
        public T Item2 { get; }

        public bool Matching (T i1, T i2)
        {
            return (Item1.Equals(i1) && Item2.Equals(i2)) || (Item1.Equals(i2) && Item2.Equals(i1));
        }
        public bool Contains(T checking)
        {
            return (Item1.Equals(checking) || Item2.Equals(checking));
        }
    }

    #endregion
    [Serializable]
    public class ItemEmpties
    {
        public SerializedDictionary<EquipItemType, Transform> ItemPositions;
    }
    #endregion




    #region interfaces

    public interface IHasEffects
    {
        public event EffectsManagerEvent BaseControllerEffectEvent;
    }

    public interface ITakesTriggers
    {
        void ApplyEffectToController(TriggeredEffect effect);
    }

    public interface IStatsComponentForHandler : IManaged
    {
        bool IsReady { get; }
        event SimpleEventsHandler<bool, IStatsComponentForHandler> ComponentChangedStateToEvent;

        // void Ping();
    }
    public interface IManaged
    {
        void UpdateInDelta(float deltaTime);
        void SetupStatsComponent();
        void StopStatsComponent();
    }

    public interface IHasID
    { string GetID { get; } }
    public interface IHasGameObject
    { public GameObject GetObject(); }
    public interface IHasOwner
    { BaseUnit Owner { get; } }

    public interface IAppliesTriggers : IHasGameObject
    {
        event SimpleTriggerEvent TriggerHitUnitEvent;
    }


    public interface IExpires : IHasGameObject
    {
        event SimpleEventsHandler<IExpires> SkillComponentExpiredEvent;
    }
    //public interface ISkill : IHasID, IAppliesTriggers, IExpires, IHasGameObject
    //{
    //    void OnUse();
    //    void OnUpdate(float delta);
    //}

    public interface INeedsEmpties
    { ItemEmpties Empties { get; } }

    #endregion


}