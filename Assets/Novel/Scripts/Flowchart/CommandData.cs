using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Novel.Command
{
    [Serializable]
    public class CommandData : ScriptableObject
    {
        [SerializeField] bool enabled = true;
        public bool Enabled => enabled;

        [SerializeField, SerializeReference, SubclassSelector]
        ICommand command;

        public async UniTask ExecuteAsync(Flowchart parentFlowchart)
        {
            if (enabled && command != null)
            {
                await command.ExecuteAsync(parentFlowchart);
            }
        }

        /// <summary>
        /// セットされているICommandのクラスを返します
        /// </summary>
        public CommandBase GetCommandBase()
        {
            if (command == null) return null;
            return command as CommandBase;
        }

#if UNITY_EDITOR

        static readonly Color NullColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        /// <summary>
        /// エディタのコマンドに状態を記述します
        /// </summary>
        public string GetSummary() => command?.GetSummary();

        /// <summary>
        /// コマンドの色を設定します
        /// </summary>
        public Color GetCommandColor() => command == null ? NullColor : command.GetCommandColor();

        /// <summary>
        /// コマンド名を取得します
        /// </summary>
        public string GetName() => command?.GetName();

        /// <summary>
        /// (CSV用)型からコマンドをセットします
        /// </summary>
        public void SetCommand(Type type)
        {
            if (type == null)
            {
                command = null;
                return;
            }
            command = Activator.CreateInstance(type) as CommandBase;
        }
#endif
    }
}