using UnityEngine;
using Cysharp.Threading.Tasks;
using Novel.Command;
using System;
using System.Collections.Generic;
#pragma warning disable 0414 // value is never used の警告を消すため

namespace Novel
{
    // FlowchartはMonoBehaviourとScriptableObjectのどちらからでも呼び出せる
    // MonoBehaviourはシーン内で参照が取れるためできることが多い
    // ScriptableObjectはどのシーンからでも呼べるので使い回しが効く
    //
    // リスト右クリックで要素をコピーできますが、そうすると参照まで共有されてしまうので、
    // 同じく右クリックから"New Instance"を押せば連携が外せます
    public class FlowchartExecutor : MonoBehaviour, IFlowchart
    {
        [SerializeField, TextArea]
        string description = "説明";

        [SerializeField] bool isStartExecute;

        // シリアライズしないとなぜかコマンドが消えたりして不安定
        [field: SerializeField, HideInInspector]
        public List<CommandData> CommandDataList { get; set; } = new();

        bool isStopped;

        void Start()
        {
            if (isStartExecute) Execute();
        }

        public void Execute(int index = 0)
        {
            ExecuteAsync(index).Forget();
        }

        public async UniTask ExecuteAsync(int index = 0, bool isNest = false)
        {
            while (CommandDataList.Count > index && isStopped == false)
            {
                var cmdData = CommandDataList[index];
                await cmdData.CallAsync(this);
                index++;
            }
            if (isStopped == false && isNest == false)
            {
                MessageBoxManager.Instance.AllFadeOutAsync().Forget();
                PortraitManager.Instance.AllFadeOutAsync().Forget();
            }
            isStopped = false;
        }

        void IFlowchart.Stop()
        {
            isStopped = true;
        }
    }
}