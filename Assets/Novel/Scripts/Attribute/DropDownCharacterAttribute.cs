using UnityEngine;
using System;

namespace Novel
{
    /// <summary>
    /// �L�����N�^�[�̃��X�g����h���b�v�_�E���őI�Ԃ��Ƃ��ł��܂�
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DropDownCharacterAttribute : PropertyAttribute
    {
        public readonly string FieldName;

        public DropDownCharacterAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}