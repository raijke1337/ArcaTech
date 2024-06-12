using Arcatech.Items;
using Arcatech.Managers;
using Arcatech.Texts;
using Arcatech.Units.Inputs;
using System.Collections;
using System.Linq;
using UnityEngine;
namespace Arcatech.Units
{
    public class PlayerUnit : BaseUnit
    {
        private InputsPlayer _playerController;
        protected Camera _faceCam;

        protected void ToggleCamera(bool value) { _faceCam.enabled = value; }

        #region managed
        public override void InitiateUnit()
        {
            UpdateComponents();
            base.InitiateUnit();
            ToggleCamera(true);

        }
 

        protected override void UpdateComponents()
        {
            base.UpdateComponents();
            if (_faceCam == null) _faceCam = GetComponentsInChildren<Camera>().First(t => t.CompareTag("FaceCamera"));
        }

        protected override void ControllerEventsBinds(bool isEnable)
        {
            base.ControllerEventsBinds(isEnable);

            
            if (isEnable)
            {
                _playerController = _controller as InputsPlayer;
                _playerController.ChangeLayerEvent += ChangeAnimatorLayer;
                _playerController.ShieldBreakHappened += HandleShieldBreakEvent;
            }
            else if (_playerController != null) // issue in scenes
            {                
                _playerController.ChangeLayerEvent -= ChangeAnimatorLayer;
                _playerController.ShieldBreakHappened -= HandleShieldBreakEvent;
            }
        }

        #endregion
        #region inventory

        protected override void InitInventory()
        {
            var savedEquips = DataManager.Instance.GetCurrentPlayerItems;

            GetUnitInventory = new UnitInventoryComponent(savedEquips, this);
            CreateStartingEquipments(GetUnitInventory);
        }
        #endregion
        #region jump
        protected override void StartJump()
        {
            if (_controller.DebugMessage)
            {
                Debug.Log("Start jump");
            }
            _rigidbody.AddForce((_playerController.transform.forward + _rigidbody.transform.up) * _debugJumpForceMult, ForceMode.Impulse);
            airTimer = StartCoroutine(AirTimeTimer());
            _animator.SetTrigger("JumpStart");
        }
        private Coroutine airTimer;

        private IEnumerator AirTimeTimer()
        {
            float time = 0;
            while (true)
            {
                time += Time.deltaTime;
                _animator.SetFloat("AirTime", time);
                yield return null;
            }
        }

        protected override void LandJump(string tag)
        {
            if ((tag == "Ground" || tag == "SolidItem") && airTimer != null)
            {
                if (_controller.DebugMessage)// && IsAirborne)
                {
                    Debug.Log($"Land  jump on tag {tag}");
                }
                StopCoroutine(airTimer);
                airTimer = null;
                _animator.SetFloat("AirTime", 0);
                _animator.SetTrigger("JumpEnd");
            }
        }

        #endregion

        #region animator

        private void ChangeAnimatorLayer(RuntimeAnimatorController animator)
        {
            _animator.runtimeAnimatorController = animator;
        }

        public void ComboWindowStart()
        {
            _animator.SetBool("AdvancingCombo", true);
            _playerController.IsInMeleeCombo = true;
        }
        public void ComboWindowEnd()
        {
            _animator.SetBool("AdvancingCombo", false);
            _playerController.IsInMeleeCombo = false;
        }


        protected override void ZeroAnimatorFloats()
        {
            base.ZeroAnimatorFloats();
            _animator.SetFloat("AirTime", 0);
        } 
        private void HandleShieldBreakEvent()
        {if (_controller.DebugMessage)
            Debug.Log($"Inputs report shield break event");
        }
        public void PlayerIsTalking(DialoguePart d)
        {
            if (_controller.DebugMessage)
                Debug.Log($"Dialogue window is shown by player panel script, dialogue is: {d.Mood}");
        }

        protected override void HandleDamage(float value)
        {
            _animator.SetTrigger("TakeDamage");

            if (_controller.DebugMessage)
                Debug.Log($"Player unit handle dmg event");
        }

        protected override void HandleDeath()
        {
            _animator.SetTrigger("KnockDown");
            if (_controller.DebugMessage)
                Debug.Log($"Player unit handle death event");
        }

        public override ReferenceUnitType GetUnitType()
        {
            return ReferenceUnitType.Player;
        }

        protected override void OnItemAdd(Item i)
        {
            _animator.SetTrigger("ItemPickup");
            if (_controller.DebugMessage)
            {
                Debug.Log($"Player picked up item {i}");
            }
        }
    }
    #endregion
}