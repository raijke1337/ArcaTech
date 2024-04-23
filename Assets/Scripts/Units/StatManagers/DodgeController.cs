using Arcatech.Effects;
using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Units
{

    [Serializable]
    public class DodgeController : BaseControllerConditional, IStatsComponentForHandler, ITakesTriggers

    { // ALL LOGIC FOR TIMERS ETC MOVED TO DODGE SKILL CTRL OBJECT (Skill controller)
        public DodgeController(ItemEmpties em, BaseUnit ow) : base(em, ow)
        {

        }
       //Dictionary<DodgeStatType, StatValueContainer> _stats;

        

        #region conditional

        protected override void FinishItemConfig(EquipmentItem item)
        {

            DodgeSkillConfigurationSO cfg = (DodgeSkillConfigurationSO)item.Skill;

            if (cfg == null)
            {
                IsReady = false;
                // throw new Exception($"Mising cfg by ID {item.ID} from item {item} : {this}");
            }

            _booster = _equipment[EquipItemType.Booster];
        }
        protected override void InstantiateItem(EquipmentItem i)
        {
            i.SetItemEmpty(Empties.ItemPositions[EquipItemType.Booster]);
        }
        #endregion

        #region managed
        public override void SetupStatsComponent()
        {
            if (!IsReady) // set ready by running OnItemAssign
            {
                // Debug.Log($"{this} is not ready for setup, items: {_equipment.Values.Count}");
                return;
            }
        }

        public override void UpdateInDelta(float deltaTime)
        {
            //foreach (var timer in _timerQueue.ToList()) timer.TimerTick(deltaTime);
            base.UpdateInDelta(deltaTime);
        }

        #endregion

        #region ctrl

        //private Queue<Timer> _timerQueue = new Queue<Timer>();
        private EquipmentItem _booster;

        public override string GetUIText
        {
            get => "";
        }
        

        //public bool IsDodgePossibleCheck()
        //{
        //    Debug.Log($"Check dodge in dodge ctrl, {_stats[DodgeStatType.Charges]} charges ");
        //    if (!IsReady) return false;


        //    if (_stats[DodgeStatType.Charges].GetCurrent == 0f) return false;
        //    else
        //    {
        //        _stats[DodgeStatType.Charges].ChangeCurrent(-1);
        //        var t = new Timer(_booster.Skill.Cooldown);
        //        _timerQueue.Enqueue(t);
        //        t.TimeUp += T_TimeUp;

        //       // EffectEventCallback(new EffectRequestPackage(_booster.Effects, EffectMoment.OnStart, _booster.GetInstantiatedPrefab().transform));

        //        return true;
        //    }
        //}

        //private void T_TimeUp(Timer arg)
        //{
        //    _timerQueue.Dequeue();
        //    _stats[DodgeStatType.Charges].ChangeCurrent(1);
        //    arg.TimeUp -= T_TimeUp; 
        //}


        protected override StatValueContainer SelectStatValueContainer(TriggeredEffect effect)
        {
            return null ;
        }


        #endregion

    }
}