using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;

namespace Novel
{
    using LinkedPortrait = PortraitsData.LinkedPortrait;

    // 基本的な実装はMessageBoxManagerと同じ
    public class PortraitManager : SingletonMonoBehaviour<PortraitManager>
    {
        [SerializeField] PortraitsData data;

        protected override void Awake()
        {
            base.Awake();
            InitCheck();
            SceneManager.activeSceneChanged += OnSceneChanged;
            OnSceneChanged();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        void InitCheck()
        {
            int enumCount = Enum.GetValues(typeof(PortraitType)).Length;

            if (data.LinkedPortraitList.Count != enumCount)
            {
                Debug.LogWarning($"{nameof(PortraitManager)}に登録している数が{nameof(PortraitType)}の数と合いません！");
            }
            else
            {
                for (int i = 0; i < enumCount; i++)
                {
                    data.LinkedPortraitList[i].Type = (PortraitType)i;
                }
            }
        }

        void OnSceneChanged(Scene _ = default, Scene __ = default)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            var existPortraits = FindObjectsByType<Portrait>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var linkedPortrait in data.LinkedPortraitList)
            {
                linkedPortrait.Portrait = null;
                foreach (var existPort in existPortraits)
                {
                    if (existPort.Type == linkedPortrait.Type)
                    {
                        existPort.gameObject.SetActive(false);
                        linkedPortrait.Portrait = existPort;
                        break;
                    }
                }

                if (linkedPortrait.Portrait == null && data.CreateOnSceneChanged)
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
            linkedPortrait.Portrait = newPortrait;
            return newPortrait;
        }

        /// <summary>
        /// ポートレートを返します。なければ生成してから返します
        /// </summary>
        public Portrait CreateIfNotingPortrait(PortraitType portraitType)
        {
            var linkedPortrait = data.LinkedPortraitList[(int)portraitType];
            if (linkedPortrait.Portrait != null) return linkedPortrait.Portrait;
            return CreateAndAddPortrait(linkedPortrait);
        }

        public async UniTask AllClearFadeAsync(float time = MyStatic.DefaultFadeTime)
        {
            foreach (var linkedPortrait in data.LinkedPortraitList)
            {
                if (linkedPortrait.Portrait == null) continue;
                linkedPortrait.Portrait.ClearFadeAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time, this.GetCancellationTokenOnDestroy());
        }
    }
}