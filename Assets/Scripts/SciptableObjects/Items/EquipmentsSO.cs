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

[CreateAssetMenu(fileName = "New Equipment Preset", menuName = "Equipments/Equipment", order = 1)]
public class EquipmentsSO : ScriptableObjectID
{
    public string[] itemIDs;
}

