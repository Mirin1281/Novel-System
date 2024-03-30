using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;
#pragma warning disable 0414 // description��"value is never used"�x������������

namespace Novel
{
    [Serializable]
    public class Flowchart
    {
        [SerializeField, TextArea]
        string description = "����";

        // �V���A���C�Y����
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();

        public List<CommandData> GetCommandDataList() => commandDataList;

        public List<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        /// <summary>
        /// �J�X�^���G�f�B�^�p
        /// </summary>
        public void SetCommandDataList(List<CommandData> list)
        {
            commandDataList = list;
        }

        bool isStopped;
        CancellationTokenSource cts;

        /// <summary>
        /// �t���[�`���[�g���Ăяo���܂�
        /// </summary>
        /// <param name="index">���X�g�̉��Ԗڂ��甭�΂��邩</param>
        /// <param name="callStatus">���̃t���[�`���[�g����Ăяo���ꂽ���̏��</param>
        public async UniTask ExecuteAsync(int index, FlowchartCallStatus callStatus)
        {
            var status = SetStatus(callStatus);

            while (commandDataList.Count > index && isStopped == false)
            {
                var cmdData = commandDataList[index];
                await cmdData.ExecuteAsync(this, status);
                index++;
            }
            bool isEndClearUI = isStopped == false && status.IsNestCalled == false;
            if (isEndClearUI)
            {
                MessageBoxManager.Instance.AllClearFadeAsync().Forget();
                PortraitManager.Instance.AllClearFadeAsync().Forget();
            }
            isStopped = false;


            /// <summary>
            /// FlowchartCallStatus��cts�ɔ��f���܂��B
            /// status��null�������ꍇ�͏����������܂�
            /// </summary>
            FlowchartCallStatus SetStatus(FlowchartCallStatus callStatus)
            {
                if (callStatus == null)
                {
                    cts = new CancellationTokenSource();
                    cts = CancellationTokenSource.CreateLinkedTokenSource(
                            MyStatic.TokenOnSceneChange, cts.Token);
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
        /// �R�}���h���X�g�̐؂�ڂŃt���[�`���[�g���~���܂�
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
    }

    public enum FlowchartStopType
    {
        [InspectorName("���̃t���[�`���[�g�̂�")] Single,
        [InspectorName("����q�̐e���܂ޑS��")] All,
    }

    /// <summary>
    /// Token, TokenSource�Ɠ���q�ŌĂ΂ꂽ����3�̏����i�[���܂�
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