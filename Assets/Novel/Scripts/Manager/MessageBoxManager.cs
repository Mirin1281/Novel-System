using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LinkedBox = Novel.MessageBoxesData.LinkedObject;

namespace Novel
{
    // 【ふるまいの雑な説明】
    // メッセージボックスをBoxTypeに応じてプレハブから生成、提供します
    // 既にシーンの中にボックスがある場合はそれを使います(名前で検索してます)ので、オーバーライドできます
    // シーンの中ではボックスはキャッシュされますが、シーン遷移するとリセットされます
    public class MessageBoxManager : SingletonMonoBehaviour<MessageBoxManager>
    {
        [SerializeField] MessageBoxesData data;

        protected override void Awake()
        {
            base.Awake();
            InitCheck();
            SceneManager.activeSceneChanged += NewFetchBoxes;
            //OnSceneChanged(); // 一番最初は2回分呼ばれるので注意
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            SceneManager.activeSceneChanged -= NewFetchBoxes;
        }

        void InitCheck()
        {
            int enumCount = Enum.GetValues(typeof(BoxType)).Length;

            // 登録数のチェック
            if (data.GetListCount() != enumCount)
            {
                Debug.LogWarning($"{nameof(MessageBoxManager)}に登録している数が{nameof(BoxType)}の数と合いません！");
            }
            else
            {
                for (int i = 0; i < enumCount; i++)
                {
                    data.GetLinkedObject(i).SetType((BoxType)i);
                }
            }
        }

        void NewFetchBoxes(Scene _ = default, Scene __ = default)
        {
            // 生成している子を削除する
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            var existBoxes = FindObjectsByType<MessageBox>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var linkedBox in data.GetLinkedObjectEnumerable())
            {
                linkedBox.Object = null;
                foreach(var existBox in existBoxes)
                {
                    if (existBox.IsTypeEqual(linkedBox.Type))
                    {
                        existBox.gameObject.SetActive(false);
                        linkedBox.Object = existBox;
                        break;
                    }
                }

                if (linkedBox.Object == null && data.CreateOnSceneChanged)
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
            linkedBox.Object = newBox;
            return newBox;
        }

        /// <summary>
        /// メッセージボックスを返します。なければ生成してから返します
        /// </summary>
        public MessageBox CreateIfNotingBox(BoxType boxType)
        {
            var linkedBox = data.GetLinkedObject((int)boxType);
            if (linkedBox.Object != null) return linkedBox.Object;
            return CreateAndAddBox(linkedBox);
        }

        public async UniTask AllClearFadeAsync(float time = ConstContainer.DefaultFadeTime, CancellationToken token = default)
        {
            foreach(var linkedBox in data.GetLinkedObjectEnumerable())
            {
                if (linkedBox.Object == null) continue;
                linkedBox.Object.ClearFadeAsync(time, token).Forget();
            }
            await Wait.Seconds(time, token == default ? this.GetCancellationTokenOnDestroy() : token);
        }

        public async UniTask OtherClearFadeAsync(BoxType boxType, float time = ConstContainer.DefaultFadeTime)
        {
            foreach (var linkedBox in data.GetLinkedObjectEnumerable())
            {
                if (linkedBox.Object == null ||
                    linkedBox.Type == boxType) continue;
                linkedBox.Object.ClearFadeAsync(time).Forget();
            }
            await Wait.Seconds(time, this.GetCancellationTokenOnDestroy());
        }
    }
}