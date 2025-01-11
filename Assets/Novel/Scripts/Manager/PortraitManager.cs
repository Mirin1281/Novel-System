using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;

namespace Novel
{
    using LinkedPortrait = PortraitsData.LinkedObject;

    // 基本的な実装はMessageBoxManagerと同じ
    public class PortraitManager : SingletonMonoBehaviour<PortraitManager>
    {
        [SerializeField] PortraitsData data;

        protected override void Awake()
        {
            base.Awake();
            InitCheck();
            SceneManager.activeSceneChanged += NewFetchPortraits;
#if UNITY_EDITOR
            // EnterPlayModeSettingsのReloadSceneがfalseであれば読み込む
            if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled
             && UnityEditor.EditorSettings.enterPlayModeOptions.HasFlag(UnityEditor.EnterPlayModeOptions.DisableSceneReload))
            {
                NewFetchPortraits();
            }
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.activeSceneChanged -= NewFetchPortraits;
        }

        void InitCheck()
        {
            int enumCount = Enum.GetValues(typeof(PortraitType)).Length;

            if (data.GetListCount() != enumCount)
            {
                Debug.LogWarning($"{nameof(PortraitsData)}に登録している数が{nameof(PortraitType)}の数と合いません！");
            }
            else
            {
                for (int i = 0; i < enumCount; i++)
                {
                    data.GetLinkedObject(i).SetType((PortraitType)i);
                }
            }
        }

        void NewFetchPortraits(Scene _ = default, Scene __ = default)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            var existPortraits = FindObjectsByType<Portrait>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var linkedPortrait in data.GetLinkedObjectEnumerable())
            {
                linkedPortrait.Object = null;
                foreach (var existPort in existPortraits)
                {
                    if (existPort.IsTypeEqual(linkedPortrait.Type))
                    {
                        existPort.gameObject.SetActive(false);
                        linkedPortrait.Object = existPort;
                        break;
                    }
                }

                if (linkedPortrait.Object == null && data.CreateOnSceneChanged)
                {
                    CreateAndAddPortrait(linkedPortrait);
                }
            }
        }

        Portrait CreateAndAddPortrait(LinkedPortrait linkedPortrait)
        {
            if (linkedPortrait.Prefab == null) return null;
            var newPortrait = Instantiate(linkedPortrait.Prefab, transform);
            newPortrait.gameObject.SetActive(false);
            newPortrait.name = linkedPortrait.Prefab.name;
            linkedPortrait.Object = newPortrait;
            return newPortrait;
        }

        /// <summary>
        /// ポートレートを返します。なければ生成してから返します
        /// </summary>
        public Portrait CreateIfNotingPortrait(PortraitType portraitType)
        {
            var linkedPortrait = data.GetLinkedObject((int)portraitType);
            if (linkedPortrait.Object != null) return linkedPortrait.Object;
            return CreateAndAddPortrait(linkedPortrait);
        }

        public async UniTask AllClearFadeAsync(float time = ConstContainer.DefaultFadeTime)
        {
            foreach (var linkedPortrait in data.GetLinkedObjectEnumerable())
            {
                if (linkedPortrait.Object == null) continue;
                linkedPortrait.Object.ClearFadeAsync(time).Forget();
            }
            await AsyncUtility.Seconds(time, this.GetCancellationTokenOnDestroy());
        }
    }
}