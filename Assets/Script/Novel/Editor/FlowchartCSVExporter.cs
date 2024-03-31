using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;
using System.IO;
using System.Text;

namespace Novel
{
	using Novel.Command;

    public static class FlowchartCSVExporter
    {
		readonly static string outPutPath = "Assets";

        public static void ExportFlowchartCommandData()
        {
            var executors = Object.FindObjectsByType<FlowchartExecutor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            executors.ToList().OrderBy(f => f.name);

			FileInfo fi = new FileInfo(outPutPath);
			StreamWriter sw = fi.CreateText();

			string header = $"FlowchartContents<{SceneManager.GetActiveScene().name}>";
			sw.WriteLine(header);

			int maxFlowchartsCmdIndex = executors.Max(f => f.Flowchart.GetReadOnlyCommandDataList().Count);

			for(int i = 0; i < maxFlowchartsCmdIndex; i++)
            {
				var sb = new StringBuilder();
				foreach (var executor in executors)
				{
					var list = executor.Flowchart.GetReadOnlyCommandDataList();
					if (i >= list.Count) continue;
					var cmdData = list[i];
					var cmdBase = cmdData.GetCommandBase();

					string cmdName = cmdData.GetCommandStatus().Name;
					string characterName = cmdBase switch
					{
						SayCommand say => say.GetCharacterName(),
						_ => string.Empty,
					};
					string content = cmdBase switch
					{
						SayCommand say => say.GetText(),
						_ => string.Empty,
					};

					
					sb.Append(cmdName).Append(",")
					.Append(characterName).Append(",")
					.Append(content).Append(",");
				}
				sw.Write(sb.ToString());
			}

			sw.Flush();
			sw.Close();
		}
    }
}