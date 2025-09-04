using KBCore.Refs;
using UnityEngine;

namespace Arcatech.Items
{

    public abstract class BaseWeaponComponent : BaseItemComponent
    {
        [SerializeField, Self] Animator animator;
        public override void OnUse()
        {
            animator.SetTrigger("Use");
        }
    }

}