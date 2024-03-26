using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Novel
{
    // 【ふるまいの雑な説明】
    // メッセージボックスをBoxTypeに応じてプレハブから生成、提供します
    // 既にシーンの中にボックスがある場合はそれを使います(名前で検索してます)ので、オーバーライドできます
    // シーンの中ではボックスはキャッシュされます
    // 基本はデフォルトのボックスで、変化を加えたい場合はシーンに置いてできる設計です
    public class MessageBoxManager : SingletonMonoBehaviour<MessageBoxManager>
    {
        [SerializeField, Tooltip("true時はシーン切り替え時に全メッセージボックスを生成します\n" +
            "false時は受注生産方式でキャッシュします")]
        bool allCreateOnSceneChanged;

        #region Data
        [Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます")]
        [SerializeField] List<LinkedBox> linkedBoxList;

        MessageBox GetBoxPrefab(BoxType type)
            => linkedBoxList[(int)type].BoxPrefab;

        [Serializable]
        class LinkedBox
        {
            [SerializeField, ReadOnly] BoxType type;
            public BoxType Type => type;
            [SerializeField] MessageBox boxPrefab;
            public MessageBox BoxPrefab => boxPrefab;
            public MessageBox Box { get; set; }

#if UNITY_EDITOR
            public void SetType(BoxType type)
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
            int enumCount = Enum.GetValues(typeof(BoxType)).Length;
            if (linkedBoxList == null) linkedBoxList = new();
            int deltaCount = 1; // 仮置き
            while (deltaCount != 0)
            {
                deltaCount = linkedBoxList.Count - enumCount;
                if (deltaCount > 0)
                {
                    linkedBoxList.RemoveAt(enumCount);
                }
                else if (deltaCount < 0)
                {
                    linkedBoxList.Add(new LinkedBox());
                }
            }

            for (int i = 0; i < enumCount; i++)
            {
                linkedBoxList[i].SetType((BoxType)i);
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

            foreach (var linkedBox in linkedBoxList)
            {
                linkedBox.Box = null;
                var boxObj = MyStatic.FindIncludInactive(linkedBox.BoxPrefab.name);
                if (boxObj != null)
                {
                    boxObj.SetActive(false);
                    linkedBox.Box = boxObj.GetComponent<MessageBox>();
                }
                else if (allCreateOnSceneChanged)
                {
                    var box = Instantiate(linkedBox.BoxPrefab, transform);
                    box.gameObject.SetActive(false);
                    box.name = linkedBox.BoxPrefab.name;
                    linkedBox.Box = box;
                }
            }
        }

        /// <summary>
        /// メッセージボックスを返します。なければ生成してから返します
        /// </summary>
        /// <param name="boxType"></param>
        /// <returns></returns>
        public MessageBox CreateIfNotingBox(BoxType boxType)
        {
            var linkedBox = linkedBoxList[(int)boxType];
            if (linkedBox.Box != null) return linkedBox.Box;

            var box = Instantiate(GetBoxPrefab(boxType), transform);
            box.gameObject.SetActive(false);
            box.name = linkedBox.BoxPrefab.name;
            linkedBox.Box = box;
            return box;
        }

        public async UniTask AllFadeOutAsync(float time = MyStatic.DefaultFadeTime)
        {
            foreach(var linkedBox in linkedBoxList)
            {
                if (linkedBox.Box == null) continue;
                linkedBox.Box.FadeOutAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time);
        }

        public async UniTask FadeOutOtherAsync(BoxType boxType, float time = MyStatic.DefaultFadeTime)
        {
            foreach (var linkedBox in linkedBoxList)
            {
                if (linkedBox.Type == boxType) continue;
                if (linkedBox.Box == null) continue;
                linkedBox.Box.FadeOutAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time);
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    public enum BoxType
    {
        [InspectorName("デフォルト")] Default,
        [InspectorName("下")] Type1,
        [InspectorName("上")] Type2,
    }
}