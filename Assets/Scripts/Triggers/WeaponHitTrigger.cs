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
using Zenject;

public class WeaponHitTrigger : BaseTrigger
{
    protected override void OnTriggerEnter(Collider other)
    { 
        base.OnTriggerEnter(other);
    }
}
