using Arcatech.BlackboardSystem;
using Arcatech.EventBus;
using Arcatech.Units;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Arcatech.Managers
{
    public class UnitsManager : MonoBehaviour, IManagedController
    {

        #region pause handling

        EventBinding<PauseToggleEvent> _pauseBind;
        private void OnPawsToggle(PauseToggleEvent isPausing)
        {
            _player.UnitPaused = !_player.UnitPaused;
            Debug.Log($"Paws the game: {_player.UnitPaused}");
            foreach(var e in entities)
            {
                e.UnitPaused = _player.UnitPaused;
            }
        }
        #endregion

        public static UnitsManager Instance;
        private void Awake()
        {
            if (Instance != null) Destroy(Instance.gameObject);
            Instance = this;
        }

        public void TryRegisterEntity(BaseEntity entity)
        {
            if (entity == null) return;
            if (entities.Contains(entity)) return;
            else
            {
                entities.Add(entity);
                SetupUnit(entity,true);
            }
        }
        public void TryDeregisterEntity(BaseEntity entity)
        {

        }


        #region managed
        public virtual void StartController()
        {

            List<BaseEntity> l = new List<BaseEntity>();
            foreach (BaseEntity u in FindObjectsOfType<BaseEntity>())
            {
                SetupUnit(u, true);
                entities.Add(u);
                if (u is PlayerUnit p)
                {
                    _player = p;
                }
            }
           // SetupBlackboard();
        }
        public virtual void ControllerUpdate(float delta)
        {
            if (_player.UnitPaused) return;

            for (int i = entities.Count - 1; i >= 0; i--)
            {
                entities[i].RunUpdate(delta);
            }
            //UpdateBlackboardController();
        }

        public virtual void FixedControllerUpdate(float fixedDelta)
        {
            if (_player.UnitPaused) return;

            for (int i = entities.Count - 1; i >= 0; i--)
            {
                entities[i].RunFixedUpdate(fixedDelta);
            }

        }

        public virtual void StopController()
        {
            foreach (var e in entities)
            {
                e.DisableUnit();
                SetupUnit(e, false);
            }
            entities.Clear();
        }
        #endregion

        #region units handling

        private PlayerUnit _player;
        List<BaseEntity> entities = new List<BaseEntity>();

        private void SetupUnit(BaseEntity u, bool isEnable)
        {
            if (isEnable)
            {
                u.BaseEntityDeathEvent += (t) => HandleUnitDeath(t);
                u.StartControllerUnit();
            }
            else
            {
                u.BaseEntityDeathEvent -= (t) => HandleUnitDeath(t);
                u.DisableUnit();
            }
        }


        private void HandleUnitDeath(BaseEntity unit)
        {

            SetupUnit(unit, false);
            entities.Remove(unit);
            
            if (unit is PlayerUnit)
            {
                GameInterfaceManager.Instance.GameOver();
            }
            else
            {
                Destroy(unit.gameObject, 2f);
            }

        }
        #endregion

        private void OnEnable()
        {
            _pauseBind = new EventBinding<PauseToggleEvent>(OnPawsToggle);
            EventBus<PauseToggleEvent>.Register(_pauseBind);
        }
        private void OnDisable()
        {
            EventBus<PauseToggleEvent>.Deregister(_pauseBind);
        }

    }

}