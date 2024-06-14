using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;

namespace Novel
{
    // Flowchart��MonoBehaviour�^��ScriptableObject�^������
    // MonoBehaviour�̓V�[�����ŎQ�Ƃ����邽�߂ł��邱�Ƃ�����
    // ScriptableObject�͂ǂ̃V�[������ł��Ăׂ�̂Ŏg���񂵂�����
    
    [Serializable]
    public class Flowchart
    {
        [SerializeField, TextArea]
        string description = "����";

        [SerializeField, Tooltip("Zone�R�}���h���g�p����ꍇ�̂�true�ɂ��Ă�������")]
        bool isCheckZone;

        // �V���A���C�Y����
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        bool isStopped;

        CancellationTokenSource cts;

        int callIndex;

        /// <summary>
        /// ���݌Ă΂�Ă���t���[�`���[�g�̃��X�g
        /// </summary>
        public static List<Flowchart> CurrentExecutingFlowcharts;

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

        /// <summary>
        /// �t���[�`���[�g���Ăяo���܂�
        /// </summary>
        /// <param name="index">���X�g�̉��Ԗڂ��甭�΂��邩</param>
        /// <param name="callStatus">���̃t���[�`���[�g����Ăяo���ꂽ���ɓn�������</param>
        public async UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus = null)
        {
            if(CurrentExecutingFlowcharts == null)
            {
                CurrentExecutingFlowcharts = new(3);
            }
            CurrentExecutingFlowcharts.Add(this);
            SetIndex(index, false);
            if (isCheckZone) ApplyZone(commandDataList, callIndex);
            var status = SetStatus(callStatus, ref cts);

            while (commandDataList.Count > callIndex && isStopped == false)
            {
                var cmdData = commandDataList[callIndex];
                await cmdData.ExecuteAsync(this, status);
                callIndex++;
            }

            bool isEndClearUI = isStopped == false && status.IsNestCalled == false;
            if (isEndClearUI)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            isStopped = false;
            CurrentExecutingFlowcharts.Remove(this);


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
                            zoneCommand.CallZone();
                        }
                    }
                }
            }

            // FlowchartCallStatus��cts�ɔ��f���܂��B
            // status��null�������ꍇ�͏����������܂�
            static FlowchartCallStatus SetStatus(FlowchartCallStatus callStatus, ref CancellationTokenSource cts)
            {
                if (callStatus == null)
                {
                    cts = new();
                    return new FlowchartCallStatus(cts.Token, cts, false);
                }
                else
                {
                    cts = callStatus.Cts;
                    return callStatus;
                }
            }
        }

        public enum StopType
        {
            [InspectorName("���̃t���[�`���[�g�̂�")] Single,
            [InspectorName("����q�̐e���܂ޑS��")] All,
        }

        /// <summary>
        /// �R�}���h���X�g�̐؂�ڂŃt���[�`���[�g���~���܂�
        /// </summary>
        public void Stop(StopType stopType)
        {
            if (stopType == StopType.All)
            {
                cts?.Cancel();
            }
            else if (stopType == StopType.Single)
            {
                isStopped = true;
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
        UniTask ExecuteAsync(int index = 0);
    }

    /// <summary>
    /// Token, TokenSource��"����q�ŌĂ΂ꂽ��"��3�̏���ێ����܂�
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