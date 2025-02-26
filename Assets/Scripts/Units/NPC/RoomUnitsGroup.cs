using Arcatech.BlackboardSystem;
using Arcatech.Units;
using com.cyborgAssets.inspectorButtonPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Arcatech.AI
{
    public class RoomUnitsGroup : MonoBehaviour 
    {
        //public List<NPCUnit> GetAllUnits => _units;
        private List<NPCUnit> _units;

        Collider box;
        [ProButton]
        void StopCombat()
        {
            foreach (var unit in _units)
            {
                unit.UnitInCombatState = false;
            }
        }

        private void OnValidate()
        {            
            Assert.IsNotNull(GetComponent<Collider>());
        }

        public void Start()
        {
            box = GetComponent<Collider>();
            box.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {            
            if (other.gameObject.TryGetComponent<NPCUnit>(out var u))
            {
                if (_units == null) _units = new List<NPCUnit>();
                if (!_units.Contains(u))
                {
                    _units.Add(u);
                    u.OnUnitAttackedEvent += Unit_OnUnitAttackedEvent;
                    u.BaseEntityDeathEvent += RemoveUnitOnDeath;
                    u.SetUnitsGroup(this);
                    Debug.Log($"{this.gameObject} register unit {u}");
                }
            }
        }
        private void Unit_OnUnitAttackedEvent(NPCUnit arg)
        {
            //placeholder
            foreach (var unit in _units) { unit.UnitInCombatState = true; };
        }


        private void RemoveUnitOnDeath(BaseEntity u)
        {
            if (u is NPCUnit unit)
            {
                _units.Remove(unit);
                unit.BaseEntityDeathEvent -= RemoveUnitOnDeath;
                unit.OnUnitAttackedEvent -= Unit_OnUnitAttackedEvent;
                Debug.Log($"{this.gameObject} deregister unit {unit}");
            }
        }

        public NPCUnit ProcessTacticsRequest(ITacticsRequest r)
        {
            return r.Process(_units);
        }
    }

}