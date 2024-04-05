using System.Collections.Generic;

namespace Novel
{
    /// <summary>
    /// ���O���L�^����N���X�A�Z�[�u�̊֌W�Ń^�v���̃��X�g�ŊǗ�
    /// </summary>
    public static class SayLogger
    {
        static List<(string, string)> logList = new();

        public static void AddLog(CharacterData character, string text)
        {
            logList.Add((character.CharacterName, text));
        }

        public static List<(string, string)> GetLog() => logList;
        public static void SetLog(List<(string, string)> list) => logList = list;
    }
}