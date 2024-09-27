using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novel
{
    public class NovelManager : SingletonMonoBehaviour<NovelManager>
    {
        #region Init and Destroy

        static readonly bool initCreateInstance = true;
        static readonly bool initCreateManagers = true;
        [SerializeField] CreateManagerParam[] managerParams;

        // この属性によりAwakeより前に処理が走る(ここでしか呼ばないほうが吉)
        // managerParamsのマネージャーを生成する
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitBeforeAwake()
        {
            if (initCreateInstance == false) return;
            var managerPrefab = Resources.Load<NovelManager>(nameof(NovelManager));
            if (managerPrefab == null)
            {
                Debug.LogWarning($"{nameof(NovelManager)}の取得に失敗しました");
                return;
            }
            var novelManager = Instantiate(managerPrefab);
            novelManager.name = managerPrefab.name;
            DontDestroyOnLoad(novelManager);
            if (initCreateManagers == false) return;
            novelManager.InitCreateManagers();
        }

        public void InitCreateManagers()
        {
            foreach (var param in managerParams)
            {
                var obj = Instantiate(param.ManagerPrefab);
                obj.name = param.ManagerPrefab.name;
                obj.transform.SetParent(this.transform);
                if (param.IsInactiveOnAwake)
                {
                    obj.SetActive(false);
                }
            }
        }

        [Serializable]
        class CreateManagerParam
        {
            [field: SerializeField]
            public GameObject ManagerPrefab { get; private set; }

            [field: SerializeField, Tooltip("生成時に非アクティブにします")]
            public bool IsInactiveOnAwake { get; private set; }
        }

        #endregion

        [SerializeField] AudioSource audioSource;

        public bool OnCancelKeyDown { get; set; }

        float cancelKeyDownTime;
        void Update()
        {
            if(OnCancelKeyDown)
            {
                cancelKeyDownTime += Time.deltaTime;
            }
            else
            {
                cancelKeyDownTime = 0f;
            }

            audioSource.mute = OnSkip;
        }

        /// <summary>
        /// (テキスト表示時に使用するのでユーザーからは使用しません)
        /// キャンセルキーを長押しするとOnSkipがtrueになります
        /// </summary>
        public bool OnSkip => 0.7f < cancelKeyDownTime;

        public float DefaultWriteSpeed { get; private set; } = 2;

        public bool IsUseRuby { get; private set; } = true;

        public bool IsWholeShowText { get; private set; } = false;

        public void ClearAllUI()
        {
            MessageBoxManager.Instance.AllClearFadeAsync().Forget();
            PortraitManager.Instance.AllClearFadeAsync().Forget();
            MenuManager.Instance.ClearFadeAsync().Forget();
        }

        public void PlayOneShot(AudioClip audioClip, float volumeRate = 1f)
        {
            audioSource.PlayOneShot(audioClip, volumeRate);
        }
    }
}