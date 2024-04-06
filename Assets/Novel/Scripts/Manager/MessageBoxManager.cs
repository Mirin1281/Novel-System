using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Novel
{
    using LinkedBox = MessageBoxesData.LinkedBox;

    // 【ふるまいの雑な説明】
    // メッセージボックスをBoxTypeに応じてプレハブから生成、提供します
    // 既にシーンの中にボックスがある場合はそれを使います(名前で検索してます)ので、オーバーライドできます
    // シーンの中ではボックスはキャッシュされます
    // 基本はデフォルトのボックスで、変化を加えたい場合はシーンに置いてできる設計です
    public class MessageBoxManager : SingletonMonoBehaviour<MessageBoxManager>
    {
        [SerializeField] MessageBoxesData data;

        protected override void Awake()
        {
            base.Awake();
            InitCheck();
            SceneManager.activeSceneChanged += OnSceneChanged;
            OnSceneChanged(); // 一番最初は2回分呼ばれるので注意
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        void InitCheck()
        {
            int enumCount = Enum.GetValues(typeof(BoxType)).Length;

            // 登録数のチェック
            if (data.LinkedBoxList.Count != enumCount)
            {
                Debug.LogWarning($"{nameof(MessageBoxManager)}に登録している数が{nameof(BoxType)}の数と合いません！");
            }
            else
            {
                for (int i = 0; i < enumCount; i++)
                {
                    data.LinkedBoxList[i].Type = (BoxType)i;
                }
            }
        }

        void OnSceneChanged(Scene _ = default, Scene __ = default)
        {
            // 生成している子を削除する
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            var existBoxes = FindObjectsByType<MessageBox>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var linkedBox in data.LinkedBoxList)
            {
                linkedBox.Box = null;
                foreach(var existBox in existBoxes)
                {
                    if (existBox.IsMeetType(linkedBox.Type))
                    {
                        existBox.gameObject.SetActive(false);
                        linkedBox.Box = existBox;
                        break;
                    }
                }

                if (linkedBox.Box == null && data.CreateOnSceneChanged)
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
            var linkedBox = data.LinkedBoxList[(int)boxType];
            if (linkedBox.Box != null) return linkedBox.Box;
            return CreateAndAddBox(linkedBox);
        }

        public async UniTask AllClearFadeAsync(
            float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            foreach(var linkedBox in data.LinkedBoxList)
            {
                if (linkedBox.Box == null) continue;
                linkedBox.Box.ClearFadeAsync(time, token).Forget();
            }
            await Wait.Seconds(time, token == default ? this.GetCancellationTokenOnDestroy() : token);
        }

        public async UniTask OtherClearFadeAsync(BoxType boxType, float time = ConstContainer.DefaultFadeTime)
        {
            foreach (var linkedBox in data.LinkedBoxList)
            {
                if (linkedBox.Box == null ||
                    linkedBox.Type == boxType) continue;
                linkedBox.Box.ClearFadeAsync(time).Forget();
            }
            await Wait.Seconds(time, this.GetCancellationTokenOnDestroy());
        }
    }
}