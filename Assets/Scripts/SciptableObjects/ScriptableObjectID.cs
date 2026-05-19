using System;
using UnityEngine;
namespace Arcatech
{
    [Serializable]
    public abstract class ScriptableObjectID : ScriptableObject
    {
        void Awake()
        {
            if (String.IsNullOrEmpty(ID)) ID = Guid.NewGuid().ToString();
        }

        [ReadOnlyText] public string ID;
    }
}