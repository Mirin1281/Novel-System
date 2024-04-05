using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Collections.Generic;
using Novel.Command;
#pragma warning disable 0414 // description��"value is never used"�x������������

namespace Novel
{
    // Flowchart��MonoBehaviour�^��ScriptableObject�^������
    // MonoBehaviour�̓V�[�����ŎQ�Ƃ����邽�߂ł��邱�Ƃ�����
    // ScriptableObject�͂ǂ̃V�[������ł��Ăׂ�̂Ŏg���񂵂�����
    
    [Serializable]�@public class Flowchart
    {
        [SerializeField, TextArea]
        string description = "����";
        public string Description => description;

        [SerializeField]
        bool isCheckZone;

        // �V���A���C�Y����
        [SerializeField, HideInInspector]
        List<CommandData> commandDataList = new();
        public IReadOnlyList<CommandData> GetReadOnlyCommandDataList() => commandDataList;

        bool isStopped;

        CancellationTokenSource cts;

        public int CallIndex { get; set; }

        /// <summary>
        /// �t���[�`���[�g���Ăяo���܂�
        /// </summary>
        /// <param name="index">���X�g�̉��Ԗڂ��甭�΂��邩</param>
        /// <param name="callStatus">���̃t���[�`���[�g����Ăяo���ꂽ���ɓn�������</param>
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


            // �Ăяo�����̃C���f�b�N�X�����āA���������ɑ��݂���
            // IZoneCommand�̂����R�}���h�𔭉΂��܂�
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

            // FlowchartCallStatus��cts�ɔ��f���܂��B
            // status��null�������ꍇ�͏����������܂�
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
        [InspectorName("���̃t���[�`���[�g�̂�")] Single,
        [InspectorName("����q�̐e���܂ޑS��")] All,
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