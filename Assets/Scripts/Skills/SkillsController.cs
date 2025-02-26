using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Skills
{
    [Serializable]
    public class SkillsController : ManagedControllerBase, ICombatActions
    {
        UnitInventoryController inv;
        UnitStatsController stats;
        protected Dictionary<UnitActionType, ISkill> _skills;

        public SkillsController (UnitStatsController stats, UnitInventoryController inv, EquippedUnit ow) : base (ow)
        {
            this.inv = inv;
            this.stats = stats;

            _skills = new();

            foreach (var skill in inv.GetSkills)
            {
                _skills[skill.UseActionType] = skill;
            }
        }
        public bool ActionAvailable(UnitActionType action)
        {
            return _skills.ContainsKey(action);
        }


        public bool TryUseAction(UnitActionType action, out BaseUnitAction onUse)
        {
            onUse = null;
            if (ActionAvailable(action))
            {
                bool ok = _skills[action].TryUseItem(stats, out onUse);
                if (ok)
                {
                    inv.DrawItems(_skills[action].DrawStrategy);
                    if (DebugMessage && Owner.UnitDebug) { Debug.Log($"{Owner} used skill {_skills[action]}"); }
                    return ok;
                }
            }
            if (DebugMessage && Owner.UnitDebug) { Debug.Log($"{Owner} failed to use skill {_skills[action]}"); }
            return false;
        }

        public bool CanUseAction(UnitActionType action)
        {
            try
            {
                return _skills[action].CanUseItem(stats);
            }
            catch
            { return false; }
        }


        public override void StartController()
        {
        }

        public override void ControllerUpdate(float delta)
        {
            foreach (var s in _skills.Values) s.DoUpdate(delta); 
        }

        public override void FixedControllerUpdate(float fixedDelta)
        {
        }

        public override void StopController()
        {
        }

    }
}