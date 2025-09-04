using System;
using UnityEngine;
namespace Arcatech
{
    [Serializable]
    public abstract class ScriptableObjectID : ScriptableObject
    {
        public SerializableGuid ID = SerializableGuid.NewGuid();
    }


}