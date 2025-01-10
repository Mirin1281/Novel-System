using UnityEngine;

namespace Novel
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindAnyObjectByType<T>();
                if (instance == null)
                {
                    Debug.LogWarning(typeof(T) + " is nothing");
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning(typeof(T) + " is multiple created", this);
                return;
            }
            instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                instance = null;
            }
        }
    }
}