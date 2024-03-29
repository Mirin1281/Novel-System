﻿using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [AddTypeMenu("Wait"), System.Serializable]
    public class WaitCommand : CommandBase
    {
        [SerializeField] float waitSeconds;

        protected override async UniTask EnterAsync()
        {
            await MyStatic.WaitSeconds(waitSeconds, CallStatus.Token);
        }
    }
}