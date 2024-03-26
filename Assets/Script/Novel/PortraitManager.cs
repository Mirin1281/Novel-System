using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novel
{
    public class PortraitManager : SingletonMonoBehaviour<PortraitManager>
    {
        [SerializeField, Tooltip("true時はシーン切り替え時に全ポートレートを生成します\n" +
            "false時は受注生産方式でキャッシュします")]
        bool allCreateOnSceneChanged;

        #region Data
        [Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます")]
        [SerializeField] List<LinkedPortrait> linkedPortraitList;

        Portrait GetPortraitPrefab(PortraitType type)
            => linkedPortraitList[(int)type].PortraitPrefab;

        [Serializable]
        class LinkedPortrait
        {
            [SerializeField, ReadOnly] PortraitType type;
            public PortraitType Type => type;
            [SerializeField] Portrait portraitPrefab;
            public Portrait PortraitPrefab => portraitPrefab;
            public Portrait Portrait { get; set; }
            public bool IsCreated { get; set; }

#if UNITY_EDITOR
            public void SetType(PortraitType type)
            {
                this.type = type;
            }
#endif
        }

#if UNITY_EDITOR
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
                linkedPortraitList[i].SetType((PortraitType)i);
            }
        }
#endif
        #endregion

        protected override void Awake()
        {
            base.Awake();
            OnSceneChanged(default, default);
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        void OnSceneChanged(Scene _, Scene __)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                Destroy(child.gameObject);
            }

            foreach (var linkedPortrait in linkedPortraitList)
            {
                linkedPortrait.Portrait = null;
                var portraitObj = MyStatic.FindIncludInactive(linkedPortrait.PortraitPrefab.name);
                if (portraitObj != null)
                {
                    portraitObj.SetActive(false);
                    linkedPortrait.Portrait = portraitObj.GetComponent<Portrait>();
                }
                else if (allCreateOnSceneChanged)
                {
                    var portrait = Instantiate(linkedPortrait.PortraitPrefab, transform);
                    portrait.gameObject.SetActive(false);
                    portrait.name = linkedPortrait.PortraitPrefab.name;
                    linkedPortrait.Portrait = portrait;
                }
            }
        }

        /// <summary>
        /// ポートレートを返します。なければ生成してから返します
        /// </summary>
        /// <param name="portraitType"></param>
        /// <returns></returns>
        public Portrait CreateIfNotingPortrait(PortraitType portraitType)
        {
            var linkedPortrait = linkedPortraitList[(int)portraitType];
            if (linkedPortrait.IsCreated) return linkedPortrait.Portrait;

            var portrait = Instantiate(GetPortraitPrefab(portraitType), transform);
            portrait.name = linkedPortrait.PortraitPrefab.name;
            linkedPortrait.IsCreated = true;
            linkedPortrait.Portrait = portrait;
            return portrait;
        }

        public async UniTask AllFadeOutAsync(float time = MyStatic.DefaultFadeTime)
        {
            foreach (var linkedPortrait in linkedPortraitList)
            {
                if (linkedPortrait.IsCreated == false) continue;
                linkedPortrait.Portrait.ClearFadeAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time);
        }

        public async UniTask FadeOutOtherAsync(PortraitType portraitType, float time = MyStatic.DefaultFadeTime)
        {
            foreach (var linkedPortrait in linkedPortraitList)
            {
                if (linkedPortrait.Type == portraitType) continue;
                if (linkedPortrait.IsCreated == false) continue;
                linkedPortrait.Portrait.ClearFadeAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time);
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    public enum PortraitType
    {
        [InspectorName("真白ノベル")] Type1,
        [InspectorName("河野修二")] Type2,
    }
}