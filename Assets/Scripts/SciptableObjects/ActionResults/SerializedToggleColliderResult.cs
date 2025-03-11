using Arcatech.Items;
using Arcatech.Units;
using UnityEngine;

namespace Arcatech.Actions
{
    [CreateAssetMenu(fileName = "New toggle melee collider result", menuName = "Actions/Action Result/Toggle melee collider")]
    public class SerializedToggleColliderResult : SerializedActionResult
    {
        [SerializeField] bool ResultingColliderState;
        [SerializeField, Range(0, 1f)] float Delay = 0.1f;
        public override IActionResult BuildActionResult()
        {
            return new ToggleColliderResult(ResultingColliderState,Delay);
        }

    }
    public class ToggleColliderResult : ActionResult
    {
        bool state;
        float delay;
        public ToggleColliderResult(bool p, float d)
        {
            state = p;
            delay = d;
        }

        public override void ProduceResult(BaseEntity user, BaseEntity target, Transform place)
        {
            if (user is ArmedUnit ar && ar.IsArmed(out IWeapon w))
            {
                if (w.UseStrategy is MeleeWeaponStrategy m)
                {
                    m.SwitchCollider(state,delay);
                }
            }
        }
    }

}