
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Inputs/InputReader")]
public class PlayerInputReaderObject : ScriptableObject, PlayerControls.IGameActions
{
    private PlayerControls _controls;

    public event UnityAction<InputAction.CallbackContext> Movement = delegate { };
   // public event UnityAction<InputAction.CallbackContext> Aim = delegate { };
    public event UnityAction<InputAction.CallbackContext> Jump = delegate { };
    public event UnityAction<InputAction.CallbackContext,UnitActionType> CombatAction = delegate { };
       // Melee = delegate { };
    // public event UnityAction<InputAction.CallbackContext> Ranged = delegate { };
    // public event UnityAction<InputAction.CallbackContext> DodgeSpec = delegate { };
    // public event UnityAction<InputAction.CallbackContext> MeleeSpec = delegate { };
    // public event UnityAction<InputAction.CallbackContext> RangedSpec = delegate { };
    // public event UnityAction<InputAction.CallbackContext> ShieldSpec = delegate { };
    public event UnityAction<InputAction.CallbackContext> UseAction = delegate { };
    public event UnityAction<InputAction.CallbackContext> PausePressed = delegate { };
    public event UnityAction<InputAction.CallbackContext> InspectPressed = delegate { };
    
    
    private void OnEnable()
    {
        if (_controls == null)
        {
            _controls = new PlayerControls();
            _controls.Game.SetCallbacks(this);
        }
        _controls.Enable();
    }
    private void OnDisable()
    {
        _controls.Disable();
    }

    public void OnWASD(InputAction.CallbackContext context)
    {
        Movement.Invoke(context);
    }

    public void OnUseMeleeSkill(InputAction.CallbackContext context)
    {
        CombatAction.Invoke(context,UnitActionType.MeleeSkill);
    }

    public void OnUseRangedSkill(InputAction.CallbackContext context)
    {
        CombatAction.Invoke(context,UnitActionType.RangedSkill);
    }

    public void OnUseShieldSkill(InputAction.CallbackContext context)
    {
        CombatAction.Invoke(context,UnitActionType.ShieldSkill);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        PausePressed.Invoke(context);
    }

    public void OnUseDodgeSkill(InputAction.CallbackContext context)
    {
        CombatAction.Invoke(context,UnitActionType.DodgeSkill);
    }

    public void OnMainAttack(InputAction.CallbackContext context)
    {
        CombatAction.Invoke(context,UnitActionType.Melee);
    }

    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        CombatAction.Invoke(context,UnitActionType.Ranged);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump.Invoke(context);
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        UseAction.Invoke(context);
    }

    public void OnInspect(InputAction.CallbackContext context)
    {
        InspectPressed.Invoke(context);
    }
}
