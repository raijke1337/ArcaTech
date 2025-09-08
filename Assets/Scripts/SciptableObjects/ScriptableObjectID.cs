using System;
using UnityEngine;
namespace Arcatech
{
    [Serializable]
    public abstract class ScriptableObjectID : ScriptableObject
    {
        protected virtual void Awake()
        {
            ID = SerializableGuid.NewGuid();
        }

        public SerializableGuid ID { get; private set; }
    }


}