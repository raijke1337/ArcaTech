using Arcatech.AI;
using Arcatech.Effects;
using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Triggers;
using Arcatech.Units;
using Arcatech.Units.Inputs;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Arcatech.Managers
{
    public class UnitsManager : MonoBehaviour, IManagedController
    {

        private PlayerUnit _player;
        public PlayerUnit GetPlayerUnit { get => _player; }
        private List<RoomUnitsGroup> _npcGroups;

        


        private EffectsManager _effects;
        private SkillsPlacerManager _skills;
        private TriggersManager _trigger;

        private bool _isUnitsLocked;
        public bool UnitsLocked
        {
            get => _isUnitsLocked;
            set
            {
                _player.LockUnit = value;
                if (_npcGroups != null)
                {
                    foreach (var g in _npcGroups)
                    {
                        foreach (var n in g.GetAllUnits)
                        {
                            n.LockUnit = value;
                        }
                    }
                }

                _isUnitsLocked = value;
            }
        }



        #region managed
        public virtual void StartController()
        {
            if (_effects == null)
            {
                _effects = EffectsManager.Instance;
            }
            if (_trigger == null)
            {
                _trigger = GameManager.Instance.GetGameControllers.TriggersProjectilesManager;
            }
            if (_skills == null)
            {
                _skills = GameManager.Instance.GetGameControllers.SkillsPlacerManager;
            }

            _player = FindObjectOfType<PlayerUnit>();
            if (_player != null)
            { 
                SetupUnit(_player, true);
            }

            var _rooms = FindObjectsOfType<EnemiesLevelBlockDecorComponent>();
            if (_rooms.Length != 0)
            {

                _npcGroups = new List<RoomUnitsGroup>();
                foreach (var room in _rooms)
                {
                    var g = new RoomUnitsGroup(room.GetAllUnitsInRoom);
                    _npcGroups.Add(g);
                    g.SpawnRoom = room;

                    foreach (var unit in g.GetAllUnits)
                    {
                        SetupUnit(unit, true);
                    }
                }
            }

        }
        public virtual void ControllerUpdate(float delta)
        {

            if (_isUnitsLocked) return;

            _player.RunUpdate(delta);
            if (_npcGroups == null) return;
            foreach (var g in _npcGroups)
            {
                foreach (var n in g.GetAllUnits.ToArray())
                {
                    n.RunUpdate(delta);
                }
            }
        }

        public virtual void FixedControllerUpdate(float fixedDelta)
        {
            if (_isUnitsLocked) return;

            _player.RunFixedUpdate(fixedDelta);
            if (_npcGroups == null) return;
            foreach (var g in _npcGroups)
            {
                foreach (var n in g.GetAllUnits.ToArray())
                {
                    n.RunFixedUpdate(fixedDelta);
                }
            }
        }

        public virtual void StopController()
        {

            _player.DisableUnit();
            if (_npcGroups != null)
            {
                foreach (var g in _npcGroups)
                {
                    foreach (var n in g.GetAllUnits)
                    {
                        n.DisableUnit();
                    }
                }
            }

        }
        #endregion

        #region units handling


        private void SetupUnit(DummyUnit u, bool isEnable)
        {
            if (isEnable)
            {
                u.BaseUnitDiedEvent += (t) => HandleUnitDeath(t);
                u.StartControllerUnit();
            }
            else
            {
                u.BaseUnitDiedEvent -= (t) => HandleUnitDeath(t);
                u.DisableUnit();
            }
        }


        private void HandleUnitDeath(BaseUnit unit)
        {
                unit.DisableUnit();
            unit.LockUnit = true;
            
            if (unit is PlayerUnit)
            {
                UnitsLocked = true;
                GameManager.Instance.OnPlayerDead();
            }

        }
        #endregion

    }

}