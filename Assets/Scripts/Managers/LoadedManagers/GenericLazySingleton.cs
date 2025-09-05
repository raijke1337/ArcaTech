using UnityEngine;

namespace Arcatech.Managers
{
    public class GenericLazySingleton<T> : MonoBehaviour where T: Component
    {
        protected static T instance;
        public static bool HasInstance => instance != null;
        public static T TryGetInstance() => HasInstance ? instance : null;
		public static T Instance 
		{
			get
			{
				if (instance == null)
				{
					instance = FindAnyObjectByType<T>();
                    if (instance == null)
                    {
                        var o = new GameObject(typeof(T).Name+"auto-created");
                        instance = o.AddComponent<T>();
                    }
				}
                return instance;
			}
		}
        protected virtual void Awake()
        {

        }
        protected virtual void Init()
        {
            if (!Application.isPlaying) return;
            instance = this as T;

        }
    
    }

}