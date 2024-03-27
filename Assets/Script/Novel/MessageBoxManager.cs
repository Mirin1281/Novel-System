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
    public class MessageBoxManager : SingletonMonoBehaviour<MessageBoxManager>, IInitializableManager
    {
        [SerializeField, Tooltip(
            "true時はシーン切り替え時に全メッセージボックスを生成します\n" +
            "false時は受注生産方式でキャッシュします")]
        bool createOnSceneChanged;

        #region Data

        [Header("右上の「︙」 > 「◆SetEnum」から列挙子を更新できます")]
        [SerializeField] List<LinkedBox> linkedBoxList;

        [Serializable]
        class LinkedBox
        {
            [field: SerializeField, ReadOnly]
            public BoxType Type { get; set; }

            [field: SerializeField]
            public MessageBox Prefab { get; private set; }

            public MessageBox Box { get; set; }
        }

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
                linkedBoxList[i].Type =(BoxType)i;
            }
        }
        #endregion

        void IInitializableManager.Init()
        {
            InitCheck();
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        void InitCheck()
        {
            int enumCount = Enum.GetValues(typeof(BoxType)).Length;

            // 登録数のチェック
            if (linkedBoxList.Count != enumCount)
            {
                Debug.LogWarning($"{nameof(MessageBoxManager)}に登録数が{nameof(BoxType)}の数と合いません！");
            }
            else
            {
                for (int i = 0; i < enumCount; i++)
                {
                    linkedBoxList[i].Type = (BoxType)i;
                }
            }
        }

        void OnSceneChanged(Scene _ = default, Scene __ = default)
        {
            // 生成している子を削除する
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                Destroy(child.gameObject);
            }

            var existBoxes = FindObjectsByType<MessageBox>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var linkedBox in linkedBoxList)
            {
                linkedBox.Box = null;
                foreach(var existBox in existBoxes)
                {
                    if (existBox.Type == linkedBox.Type)
                    {
                        existBox.gameObject.SetActive(false);
                        linkedBox.Box = existBox;
                        break;
                    }
                }

                if (linkedBox.Box == null && createOnSceneChanged)
                {
                    CreateAndAddBox(linkedBox);
                }
            }
        }

        MessageBox CreateAndAddBox(LinkedBox linkedBox)
        {
            if (linkedBox.Prefab == null) return null;
            var newBox = Instantiate(linkedBox.Prefab, transform);
            newBox.gameObject.SetActive(false);
            newBox.name = linkedBox.Prefab.name;
            linkedBox.Box = newBox;
            return newBox;
        }

        /// <summary>
        /// メッセージボックスを返します。なければ生成してから返します
        /// </summary>
        public MessageBox CreateIfNotingBox(BoxType boxType)
        {
            var linkedBox = linkedBoxList[(int)boxType];
            if (linkedBox.Box != null) return linkedBox.Box;
            return CreateAndAddBox(linkedBox);
        }

        public async UniTask AllClearFadeAsync(float time = MyStatic.DefaultFadeTime)
        {
            foreach(var linkedBox in linkedBoxList)
            {
                if (linkedBox.Box == null) continue;
                linkedBox.Box.ClearFadeAsync(time).Forget();
            }
            await MyStatic.WaitSeconds(time);
        }

        public async UniTask OtherClearFadeAsync(BoxType boxType, float time = MyStatic.DefaultFadeTime)
        {
            foreach (var linkedBox in linkedBoxList)
            {
                if (linkedBox.Box == null ||
                    linkedBox.Type == boxType) continue;
                linkedBox.Box.ClearFadeAsync(time).Forget();
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