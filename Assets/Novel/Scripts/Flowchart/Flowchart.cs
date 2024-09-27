using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;

namespace Novel
{
    // �y�����z
    // Flowchart��MonoBehaviour�^��ScriptableObject�^������܂�
    // MonoBehaviour�̓V�[�����ŎQ�Ƃ����邽�߂ł��邱�Ƃ������ł�
    // ScriptableObject�͂ǂ̃V�[������ł��Ăׂ�̂Ŏg���񂵂������܂�

    // Flowchart�͒��g�������ł����A���[�U�[�����g�p����̂�ExecuteAsync()��Stop()���قƂ�ǂł�
    
    [Serializable]
    public class Flowchart
    {
        public enum StopType
        {
            [InspectorName("���̃t���[�`���[�g�̂�")] Single,
            [InspectorName("�ҋ@���̐e���܂ޑS��")] All,
        }

        [SerializeField, TextArea]
        string description = "����";

        [SerializeField, Tooltip("Zone�R�}���h���g�p����ꍇ�̂�true�ɂ��Ă�������")]
        bool isCheckZone;

        // �V���A���C�Y����
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();
        bool isSingleStopped;
        bool isCalling;
        int callIndex;

        public FlowchartCallStatus CallStatus { get; set; }
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        /// <summary>
        /// �t���[�`���[�g���Ăяo���܂�
        /// </summary>
        /// <param name="index">���X�g�̉��Ԗڂ��甭�΂��邩</param>
        /// <param name="token">�L�����Z���p�̃g�[�N��</param>
        public UniTask ExecuteAsync(int index = 0, CancellationToken token = default)
        {
            SetStatus(token);
            return PrecessAsync(index);
        }

        // �ʏ�A������̓��[�U�[����Ăяo���܂���
        public UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus)
        {
            CallStatus = callStatus;
            return PrecessAsync(index);
        }

        async UniTask PrecessAsync(int index)
        {
            isCalling = true;
            SetIndex(index, false);
            if (isCheckZone) ApplyZone(commandDataList, callIndex);

            while (commandDataList.Count > callIndex && isSingleStopped == false)
            {
                var cmdData = commandDataList[callIndex];
                await cmdData.ExecuteAsync(this);
                callIndex++;
            }

            if (isSingleStopped == false && CallStatus.ExistWaitOthers == false)
            {
                NovelManager.Instance.ClearAllUI();
            }
            isSingleStopped = false;
            isCalling = false;
        }

        /// <summary>
        /// �t���[�`���[�g���~���܂�
        /// </summary>
        public void Stop(StopType stopType = StopType.All, bool isClearUI = false)
        {
            if (stopType == StopType.All)
            {
                CallStatus.Cts?.Cancel();
                if(isCalling && isClearUI)
                {
                    NovelManager.Instance.ClearAllUI();
                }
            }
            else if (stopType == StopType.Single)
            {
                isSingleStopped = true;
            }
        }

        // �R�}���h���ŌĂ΂ꂽ�ۂ͔�����ۂ�index��+1����̂ŁA���̕��\�߈����Ă���
        public void SetIndex(int index, bool calledInCommand)
        {
            if(calledInCommand)
            {
                callIndex = index - 1;
            }
            else
            {
                callIndex = index;
            }
        }

        void SetStatus(CancellationToken token)
        {
            CancellationTokenSource cts = new();
            if (token != default)
            {
                cts = CancellationTokenSource.CreateLinkedTokenSource(token, cts.Token);
            }
            CallStatus = new(cts.Token, cts, false);
        }    

        // �Ăяo�����̃C���f�b�N�X�����āA���������ɑ��݂���
        // IZoneCommand�̂����R�}���h�𔭉΂��܂�(�ڂ�����IZoneCommand�Q��)
        static void ApplyZone(IList<CommandData> commandDataList, int currentIndex)
        {
            for (int i = 0; i < commandDataList.Count; i++)
            {
                if (commandDataList[i].GetCommandBase() is IZoneCommand zoneCommand)
                {
                    if(i < currentIndex)
                    {
                        zoneCommand.CallIfInZone();
                    }
                }
            }
        }

#if UNITY_EDITOR
        public string Description => description;
        public List<CommandData> GetCommandDataList() => commandDataList;
        public void SetCommandDataList(List<CommandData> list)
        {
            commandDataList = list;
        }
#endif
    }

    public interface IFlowchartObject
    {
        string Name { get; }
        Flowchart Flowchart { get; }
        UniTask ExecuteAsync(int index = 0, CancellationToken token = default);
    }

    /// <summary>
    /// Token, TokenSource��"���̃t���[�`���[�g���I����ҋ@���Ă��邩"��3�̏���ێ����܂�
    /// </summary>
    public class FlowchartCallStatus
    {
        public readonly CancellationToken Token;
        public readonly CancellationTokenSource Cts;
        /// <summary>
        /// �ҋ@���Ă���ʂ̃t���[�`���[�g�����݂��邩
        /// </summary>
        public readonly bool ExistWaitOthers;

        public FlowchartCallStatus(CancellationToken token, CancellationTokenSource cts, bool existWaitOthers)
        {
            Token = token;
            Cts = cts;
            ExistWaitOthers = existWaitOthers;
        }
    }
}