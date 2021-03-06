using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[Serializable]
public class EnemyWeaponCtrl : WeaponController
{
    public EnemyWeaponCtrl(ItemEmpties ie) : base(ie)
    {
    }

    public override void SetupStatsComponent()
    {
        base.SetupStatsComponent();
        if (!Equip(EquipItemType.MeleeWeap)) Equip(EquipItemType.RangedWeap);
    }

}

