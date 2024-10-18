using UnityEngine;
using System.Collections.Generic;

namespace Novel
{
    // フラグに使う変数の型を増やしたい時は
    // 1. FlagKey_"型名" というScriptableObjectクラスをつくる
    // 2. 下部のGetFlagValueString()内の型判定を増やす
    // (3. 必要に応じてSet~~FlagCommandなどを追加する)
    // とすると追加できます。


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
        public static bool TryGetFlagValue<T>(FlagKeyDataBase<T> flagKey, out T value)
        {
            if(flagDictionary.TryGetValue(flagKey.GetName(), out var v))
            {
                value = (T)v;
                return true;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{flagKey.GetName()}が辞書に含まれてませんでした");
#endif
                value = default;
                return false;
            }
        }

        /// <summary>
        /// 返り値は(辞書に含まれていたか, 値の文字列)
        /// </summary>
        public static bool TryGetFlagValueString(FlagKeyDataBase flagKey, out string valueStr)
        {
            valueStr = string.Empty;
            if(flagKey is FlagKey_Bool boolKey)
            {
                if(TryGetFlagValue(boolKey, out var value))
                {
                    valueStr = value.ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (flagKey is FlagKey_Int intKey)
            {
                if(TryGetFlagValue(intKey, out var value))
                {
                    valueStr = value.ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (flagKey is FlagKey_String stringKey)
            {
                if(TryGetFlagValue(stringKey, out var value))
                {
                    valueStr = value.ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /*else if (flagKey is FlagKey_Float floatKey)
            {
                if(TryGetFlagValue(floatKey, out var value))
                {
                    valueStr = value.ToString();
                    return true;
                }
                else
                {
                    return false;
                }
            }*/

            throw new System.Exception();
        }

        public static void DebugShowDictionary()
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