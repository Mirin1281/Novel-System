using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novel
{
    // 基本的な実装はMessageBoxManagerと同じ
    public class PortraitManager : SingletonMonoBehaviour<PortraitManager>
    {
        [SerializeField] bool createOnSceneChanged;

        #region Data

        [Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます")]
        [SerializeField] List<LinkedPortrait> linkedPortraitList;

        [Serializable]
        class LinkedPortrait
        {
            [field: SerializeField, ReadOnly]
            public PortraitType Type { get; set; }
            [field: SerializeField]
            public Portrait Prefab;
            public Portrait Portrait { get; set; }
        }

        /// <summary>
        /// 列挙子を設定します
        /// </summary>
        [ContextMenu("◆SetEnum")]
        void SetEnum()
        {
            int enumCount = Enum.GetValues(typeof(PortraitType)).Length;
            if (linkedPortraitList == null) linkedPortraitList = new();
            int deltaCount = 1; // 仮置き
            while (deltaCount != 0)
            {
                deltaCount = linkedPortraitList.Count - enumCount;
                if (deltaCount > 0)
                {
                    linkedPortraitList.RemoveAt(enumCount);
                }
                else if (deltaCount < 0)
                {
                    linkedPortraitList.Add(new LinkedPortrait());
                }
            }

            for (int i = 0; i < enumCount; i++)
            {
                linkedPortraitList[i].Type =(PortraitType)i;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            InitCheck();
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        void InitCheck()
        {
            int enumCount = Enum.GetValues(typeof(PortraitType)).Length;

            if (linkedPortraitList.Count != enumCount)
            {
                Debug.LogWarning($"{nameof(PortraitManager)}に登録している数が{nameof(PortraitType)}の数と合いません！");
            }
            else
            {
                for (int i = 0; i < enumCount; i++)
                {
                    linkedPortraitList[i].Type = (PortraitType)i;
                }
            }
        }

        void OnSceneChanged(Scene _ = default, Scene __ = default)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                Destroy(child.gameObject);
            }

            var existPortraits = FindObjectsByType<Portrait>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var linkedPortrait in linkedPortraitList)
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

                if (linkedPortrait.Portrait == null && createOnSceneChanged)
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
            var linkedPortrait = linkedPortraitList[(int)portraitType];
            if (linkedPortrait.Portrait != null) return linkedPortrait.Portrait;
            return CreateAndAddPortrait(linkedPortrait);
        }

        public async UniTask AllClearFadeAsync(float time = MyStatic.DefaultFadeTime)
        {
            foreach (var linkedPortrait in linkedPortraitList)
            {
                if (linkedPortrait.Portrait == null) continue;
                linkedPortrait.Portrait.ClearFadeAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time, destroyCancellationToken);
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    public enum PortraitType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("真白ノベル")] Type1,
        [InspectorName("河野修二")] Type2,
    }
}