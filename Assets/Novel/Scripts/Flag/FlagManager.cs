using UnityEngine;
using System.Collections.Generic;

namespace Novel
{
    // フラグに使う変数の型を増やしたい時は
    // 1. 下部のGetFlagValueString()内の型判定を増やす
    // 2. FlagKey_"型名"のScriptableObjectをつくる
    // (3. 必要に応じてSet~~FlagCommandなどを追加する)

    // FlagKeyを鍵として変数をやり取りするクラス
    public static class FlagManager
    {
        static Dictionary<string, object> flagDictionary = new();

        public static void SetFlagValue<T>(FlagKeyDataBase<T> flagKey, T value)
        {
            flagDictionary[flagKey.GetName()] = value;
        }

        /// <summary>
        /// 返り値は(辞書に含まれていたか, 値)
        /// </summary>
        public static (bool, T) GetFlagValue<T>(FlagKeyDataBase<T> flagKey)
        {
            if (flagDictionary.ContainsKey(flagKey.GetName()) == false)
            {
                Debug.LogWarning($"{flagKey.GetName()}が辞書に含まれてませんでした");
                return (false, default);
            }
            return (true, (T)flagDictionary[flagKey.GetName()]);
        }

        /// <summary>
        /// 返り値は(辞書に含まれていたか, 値の文字列)
        /// </summary>
        public static (bool result, string valueStr) GetFlagValueString(FlagKeyDataBase flagKey)
        {
            if(flagKey is FlagKey_Bool boolKey)
            {
                var (result, value) = GetFlagValue(boolKey);
                return (result, value.ToString());
            }
            else if (flagKey is FlagKey_Int intKey)
            {
                var (result, value) = GetFlagValue(intKey);
                return (result, value.ToString());
            }
            else if (flagKey is FlagKey_String stringKey)
            {
                var (result, value) = GetFlagValue(stringKey);
                return (result, value);
            }
            /*else if (flagKey is FlagKey_Float floatKey)
            {
                var (result, i) = GetFlagValue(floatKey);
                return (result, i.ToString());
            }*/

            throw new System.Exception();
        }

        public static void DebugShowContents()
        {
#if UNITY_EDITOR
            foreach(var(s, o) in flagDictionary)
            {
                Debug.Log($"flag: {s}, val: {o}");
            }
#endif      
        }

        public static Dictionary<string, object> GetFlagDictionary() => flagDictionary;
        public static void SetFlagDictionary(Dictionary<string, object> dic) => flagDictionary = dic;
    }
}