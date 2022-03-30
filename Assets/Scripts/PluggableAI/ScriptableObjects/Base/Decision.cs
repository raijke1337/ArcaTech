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
public abstract class Decision : ScriptableObject
{
    public abstract bool Decide(InputsNPC controller);
    // gives a response to a state logic dilemma
}

