using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Novel
{
    public interface IEnum2ObjectListData
    {
        void SetEnums();
    }

    /// <summary>
    /// Enumを用いてObjectをやり取りできます
    /// </summary>
    /// <typeparam name="TEnum">列挙型</typeparam>
    /// <typeparam name="TObj">保存するオブジェクト</typeparam>
    public class Enum2ObjectListDataBase<TEnum, TObj> : ScriptableObject, IEnum2ObjectListData
        where TEnum : struct, Enum, IComparable, IFormattable, IConvertible
        where TObj : Object
    {
        [SerializeField] List<LinkedObject> linkedObjectList;

        public int GetListCount() => linkedObjectList.Count;
        public IEnumerable<LinkedObject> GetLinkedObjectEnumerable() => linkedObjectList;
        public LinkedObject GetLinkedObject(int index) => linkedObjectList[index];

        [Serializable]
        public class LinkedObject
        {
            [SerializeField]
            TEnum type;
            public TEnum Type => type;

            public void SetType(TEnum type)
            {
                this.type = type;
            }

            [SerializeField]
            TObj prefab;
            public TObj Prefab => prefab;

            public TObj Object { get; set; }
        }

        void IEnum2ObjectListData.SetEnums()
        {
            int enumCount = Enum.GetValues(typeof(TEnum)).Length;
            if (linkedObjectList == null) linkedObjectList = new();
            int deltaCount = 1; // 仮置き
            while (deltaCount != 0)
            {
                deltaCount = linkedObjectList.Count - enumCount;
                if (deltaCount > 0)
                {
                    linkedObjectList.RemoveAt(enumCount);
                }
                else if (deltaCount < 0)
                {
                    linkedObjectList.Add(new LinkedObject());
                }
            }

            for (int i = 0; i < enumCount; i++)
            {
                linkedObjectList[i].SetType((TEnum)Enum.ToObject(typeof(TEnum), i));
            }
        }
    }
}