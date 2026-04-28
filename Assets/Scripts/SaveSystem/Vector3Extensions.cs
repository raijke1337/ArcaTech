using UnityEngine;

namespace Arcatech.SaveSystem
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Преобразует Vector3 в SerializableVector3
        /// </summary>
        public static SerializableVector3 ToSerializable(this Vector3 vector)
        {
            return new SerializableVector3(vector);
        }

        /// <summary>
        /// Преобразует SerializableVector3 обратно в Vector3
        /// </summary>
        public static Vector3 ToVector3(this SerializableVector3 serializable)
        {
            if (serializable == null)
                return Vector3.zero;
            
            return new Vector3(serializable.x, serializable.y, serializable.z);
        }
    }
}