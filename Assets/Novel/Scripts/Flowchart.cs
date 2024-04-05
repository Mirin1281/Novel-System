using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;
#pragma warning disable 0414 // descriptionの"value is never used"警告を消すため

namespace Novel
{
    // FlowchartはMonoBehaviour型とScriptableObject型がある
    // MonoBehaviourはシーン内で参照が取れるためできることが多い
    // ScriptableObjectはどのシーンからでも呼べるので使い回しが効く
    
    [Serializable]　public class Flowchart
    {
        [SerializeField, TextArea]
        string description = "説明";
        public string Description => description;

        [SerializeField]
        bool isCheckZone;

        // シリアライズする
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        bool isStopped;

        CancellationTokenSource cts;

        public int CallIndex { get; set; }

        /// <summary>
        /// フローチャートを呼び出します
        /// </summary>
        /// <param name="index">リストの何番目から発火するか</param>
        /// <param name="callStatus">他のフローチャートから呼び出された時に渡される情報</param>
        public async UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus)
        {
            CallIndex = index;
            if (isCheckZone) ApplyZone(CallIndex);

            var status = SetStatus(callStatus);

            while (commandDataList.Count > CallIndex && isStopped == false)
            {
                //Debug.Log(CallIndex + ", " + commandDataList[CallIndex].GetCommandStatus().Name);
                var cmdData = commandDataList[CallIndex];
                await cmdData.ExecuteAsync(this, status);
                CallIndex++;
            }
            bool isEndClearUI = isStopped == false && status.IsNestCalled == false;
            if (isEndClearUI)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            isStopped = false;


            // 呼び出し時のインデックスを見て、それよりも上に存在する
            // IZoneCommandのついたコマンドを発火します
            void ApplyZone(int currentIndex)
            {
                for (int i = 0; i < commandDataList.Count; i++)
                {
                    if (commandDataList[i].GetCommandBase() is IZoneCommand zoneCmd)
                    {
                        if(i <= currentIndex)
                        {
                            zoneCmd.CallZone();
                        }
                    }
                }
            }

            // FlowchartCallStatusをctsに反映します。
            // statusがnullだった場合は初期化をします
            FlowchartCallStatus SetStatus(FlowchartCallStatus callStatus)
            {
                if (callStatus == null)
                {
                    cts = new();
                    cts = CancellationTokenSource.CreateLinkedTokenSource(
                            StaticToken.TokenOnSceneChange, cts.Token);
                    return new FlowchartCallStatus(cts.Token, cts, false);
                }
                else
                {
                    cts = callStatus.Cts;
                    return callStatus;
                }
            }
        }

        /// <summary>
        /// コマンドリストの切れ目でフローチャートを停止します
        /// </summary>
        public void Stop(FlowchartStopType stopType)
        {
            if (stopType == FlowchartStopType.All)
            {
                cts?.Cancel();
            }
            else if (stopType == FlowchartStopType.Single)
            {
                isStopped = true;
            }
        }

#if UNITY_EDITOR
        public List<CommandData> GetCommandDataList() => commandDataList;
        public void SetCommandDataList(List<CommandData> list)
        {
            commandDataList = list;
        }
#endif
    }

    public enum FlowchartStopType
    {
        [InspectorName("このフローチャートのみ")] Single,
        [InspectorName("入れ子の親も含む全て")] All,
    }

    /// <summary>
    /// Token, TokenSourceと"入れ子で呼ばれたか"の3つの情報を保持します
    /// </summary>
    public class FlowchartCallStatus
    {
        public readonly CancellationToken Token;
        public readonly CancellationTokenSource Cts;
        public readonly bool IsNestCalled;

        public FlowchartCallStatus(
            CancellationToken token, CancellationTokenSource cts, bool isNestCalled)
        {
            Token = token;
            Cts = cts;
            IsNestCalled = isNestCalled;
        }
    }
}