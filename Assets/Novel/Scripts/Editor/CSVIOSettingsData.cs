using UnityEngine;

namespace Novel.Editor
{
    [CreateAssetMenu(
        fileName = "CSVIOSettings",
        menuName = "ScriptableObject/CSVIOSettings")
    ]
    public class CSVIOSettingsData : ScriptableObject
    {
        [field: Header("CSV�t�@�C���̏o�͐�p�X"), SerializeField]
        public string CSVFolderPath { get; private set; } = "Assets";

        [field: Header("�f�t�H���g�̃t�@�C����\n\"(�V�[����)_(���̃t�@�C����)\"�Ƃ������O�ŏo�͂���܂�"), SerializeField]
        public string ExportFileName { get; private set; } = "FlowchartSheet";

        [field: Header("�����o�����A��A�N�e�B�u�̃Q�[���I�u�W�F�N�g���܂ނ�"), SerializeField]
        public FindObjectsInactive FlowchartFindMode { get; private set; } = FindObjectsInactive.Include;

        [field: Header("�����o�����A1�̃t���[�`���[�g�ɑ΂��ĂƂ��̐�"), SerializeField]
        public int RowCountPerOne { get; private set; } = 3;

        [field: Header("�ǂݍ��ݎ��ACSV�ƃt���[�`���[�g�ŃR�}���h�����قȂ�ۂɏ���������"), SerializeField]
        public bool IsChangeIfDifferentCmdName { get; private set; } = true;
    }
}