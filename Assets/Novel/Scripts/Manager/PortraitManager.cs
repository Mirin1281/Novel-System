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

            if (data.GetListCount() != enumCount)
            {
                Debug.LogWarning($"{nameof(PortraitManager)}に登録している数が{nameof(PortraitType)}の数と合いません！");
            }
            else
            {
                for (int i = 0; i < enumCount; i++)
                {
                    data.GetLinkedObject(i).SetType((PortraitType)i);
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
            foreach (var linkedPortrait in data.GetLinkedObjectEnumerable())
            {
                linkedPortrait.Object = null;
                foreach (var existPort in existPortraits)
                {
                    if (existPort.IsMeetType(linkedPortrait.Type))
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
            await Wait.Seconds(time, this.GetCancellationTokenOnDestroy());
        }
    }
}