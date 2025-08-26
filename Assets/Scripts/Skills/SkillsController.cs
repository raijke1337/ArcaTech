using Arcatech.EventBus;
using Arcatech.Items;
using Arcatech.Stats;
using Arcatech.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Arcatech.Skills
{
    [Serializable]
    public class SkillsController : ManagedControllerBase, ICombatActions
    {
        UnitInventoryController inv;
        UnitStatsControllerOLD stats;
        protected Dictionary<UnitActionType, ISkill> _skills;
        private EventBinding<InventoryUpdateEvent> bindInv;

        public SkillsController (UnitStatsControllerOLD stats, UnitInventoryController inv, EquippedUnit ow) : base (ow)
        {
            this.inv = inv;
            this.stats = stats;
            bindInv = new EventBinding<InventoryUpdateEvent>(OnInvUpdate);

            _skills = new();
            foreach (var skill in inv.GetSkills)
            {
                _skills[skill.UseActionType] = skill;
            }

        }

        private void OnInvUpdate(InventoryUpdateEvent e)
        {
            var newSkills = e.Inventory.GetSkills;
            List<UnitActionType> newTypes = new List<UnitActionType>();
            foreach (var s in newSkills)
            {
                newTypes.Add(s.UseActionType);
            }
            foreach (var type in _skills.Keys.ToList())
            {
                if (!newTypes.Contains(type))
                {
                    _skills.Remove(type);
                }
            }

            foreach (var skill in newSkills)
            {
                if (!_skills.ContainsValue(skill))
                {
                    _skills[skill.UseActionType] = skill;
                }
                else
                {
                    if (!_skills[skill.UseActionType].Equals(skill))
                    {
                        _skills[skill.UseActionType] = skill;
                    }
                }
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
            EventBus<InventoryUpdateEvent>.Register(bindInv);
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
            EventBus<InventoryUpdateEvent>.Deregister(bindInv);
        }

    }
}