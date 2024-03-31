using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System.Text;

namespace Novel
{
    public static class FlowchartCSVExporter
    {
		readonly static string fileName = "FlowchartCSV";
		readonly static string defaultPath = $"{Application.dataPath}/Resources";

		public static void ExportFlowchartCommandData()
        {
			string sceneName = SceneManager.GetActiveScene().name;
			var name = FlowchartEditorUtility.GetFileName(defaultPath, $"{fileName}_{sceneName}", "csv");
			var encoding = Encoding.GetEncoding("shift_jis");
			StreamWriter sw = new($"{defaultPath}/{name}", false, encoding);

			try
            {
				sw.WriteLine($"SceneName: {sceneName}");

				var executors = Object.FindObjectsByType<FlowchartExecutor>(
					FindObjectsInactive.Include, FindObjectsSortMode.None)
					.ToList()
					.OrderBy(f => f.name);

				int maxFlowchartsCmdIndex = executors.Max(f => f.Flowchart.GetReadOnlyCommandDataList().Count);

				var sb = new StringBuilder();
				foreach (var executor in executors)
                {
					sb.Append(executor.name).Append(",")
						.Skip(3);
				}
				sw.WriteLine(sb.ToString());
				sb.Clear();

				for (int i = 0; i < maxFlowchartsCmdIndex; i++)
				{
					
					foreach (var executor in executors)
					{
						var list = executor.Flowchart.GetReadOnlyCommandDataList();
						if (i >= list.Count)
                        {
							sb.Skip(4);
							continue;
						}
						var cmdStatus = list[i].GetCommandStatus();

						sb.Append(cmdStatus.Name).Append(",")
							.Append(cmdStatus.Content1).Append(",")
							.Append(cmdStatus.Content2).Append(",")
							.Skip(1);
					}

					sw.WriteLine(sb.ToString());
					sb.Clear();
				}
			}
			catch
            {
				sw.Flush();
				sw.Close();
				Debug.Log("ƒGƒ‰‚Á‚½");
			}
			sw.Flush();
			sw.Close();


		}
    }
	
	public static class StringBuilderExtension
    {
		public static StringBuilder Skip(this StringBuilder sb, int count)
        {
			for(int i = 0; i < count; i++)
            {
				sb.Append(string.Empty).Append(",");
			}
			return sb;
		}
	}
}