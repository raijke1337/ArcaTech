using Arcatech.Triggers;
using KBCore.Refs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arcatech.Items
{

    public abstract class BaseWeaponComponent : BaseEquippableItemComponent
    {
        [SerializeField, Self] Animator animator;
        public override void OnUse()
        {
            animator.SetTrigger("Use");
        }
    }

}