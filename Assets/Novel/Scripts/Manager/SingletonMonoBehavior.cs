using UnityEngine;

namespace Novel
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        const bool CallLog = false;

        static T instance;
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindAnyObjectByType<T>();
                if (instance == null && CallLog)
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
                if (CallLog)
#pragma warning disable CS0162 // 到達できないコードが検出されました
                    Debug.LogWarning(typeof(T) + " is multiple created", this);
#pragma warning restore CS0162 // 到達できないコードが検出されました
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