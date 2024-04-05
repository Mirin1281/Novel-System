using UnityEngine;

namespace Novel.Command
{
    /// <summary>
    /// ���O�A��ԁA�F�A������\��
    /// </summary>
    public readonly struct CommandStatus
    {
        static readonly Color NullColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        readonly public string Name;
        readonly public string Summary;
        readonly public Color Color;
        readonly public string Info;

        public CommandStatus(
            string name, string summary = null, Color color = default, string info = null)
        {
            Name = name;
            Summary = summary ?? string.Empty;
            Color = color == default ? NullColor : color;
            Info = info ?? string.Empty;
        }
    }
}