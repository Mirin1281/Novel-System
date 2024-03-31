using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System.Text;
using UnityEditor;
using System.Collections.Generic;
using Novel.Command;

namespace Novel
{
    public static class FlowchartCSVExporter
    {
		readonly static string fileName = "FlowchartCSV";
		readonly static string defaultPath = $"{Application.dataPath}/CSV";
		readonly static int rowCount = 3;

		public static void ExportFlowchartCommandData()
        {
			string sceneName = SceneManager.GetActiveScene().name;
			var name = FlowchartEditorUtility.GetFileName(defaultPath, $"{fileName}_{sceneName}", "csv");
			var encoding = Encoding.GetEncoding("shift_jis");
			StreamWriter sw = new($"{defaultPath}/{name}", false, encoding);

			try
            {
				sw.Write("SceneName,");
				sw.WriteLine(sceneName);

				var executors = Object.FindObjectsByType<FlowchartExecutor>(
					FindObjectsInactive.Include, FindObjectsSortMode.None)
					.ToList()
					.OrderBy(f => f.name);

				int maxFlowchartsCmdIndex = executors.Max(f => f.Flowchart.GetReadOnlyCommandDataList().Count);

				var sb = new StringBuilder();
				foreach (var executor in executors)
                {
					sb.Append(executor.name).Append(",")
						.Skip(rowCount);
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
							sb.Skip(rowCount + 1);
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
				Debug.Log("エラった");
			}
			sw.Flush();
			sw.Close();
		}

		public static void ImportFlowchartCommandData()
        {
			var absolutePath = EditorUtility.OpenFilePanel("Open csv", Application.dataPath, "CSV");
			if (string.IsNullOrEmpty(absolutePath)) return;
			var relativePath = AbsoluteToAssetsPath(absolutePath);

			var fs = new FileStream(relativePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			var encoding = Encoding.GetEncoding("shift_jis");
			var reader = new StreamReader(fs, encoding);

			var dataList = new List<string[]>();
			while (reader.Peek() != -1)
			{
				string line = reader.ReadLine();
				dataList.Add(line.Split(','));
			}

			reader.Close();



			var currentSceneName = SceneManager.GetActiveScene().name;
			if (currentSceneName != dataList[0][1])
            {
				Debug.LogError("シーンの名前が一致しません！");
				return;
            }
			dataList.RemoveAt(0);

			var executors = Object.FindObjectsByType<FlowchartExecutor>(
					FindObjectsInactive.Include, FindObjectsSortMode.None)
					.OrderBy(f => f.name)
					.ToList();

			var columnCount = dataList.Count; // 6
											  //var rowCount = ; // 3

			int count = 0;
			foreach(var executor in executors)
            {
				for (int i = 0; i < columnCount; i++)
				{
					string cmdName = null;
					CommandBase cmdBase = null;
					for (int k = count; k < rowCount + count; k++)
					{
						if (i == 0)
						{
							var executorName = dataList[0][0];
							if (executorName != executor.name)
							{
								Debug.LogWarning("フローチャートの名前が合いませんでした！ " + executorName);
							}
							break;
						}
						else
                        {
							if(k == 0)
                            {
								var cmdStatusList = executor.Flowchart.GetReadOnlyCommandDataList()
									.Select(data => data.GetCommandStatus()).ToArray();
								cmdBase = executor.Flowchart.GetReadOnlyCommandDataList()[i].GetCommandBase();
								cmdName = dataList[i][k];
								if (cmdName != cmdStatusList[i].Name)
                                {
									Debug.LogWarning($"コマンドの名前が合いませんでした！\n" +
										$"csv: {dataList[i][k]}, object: {cmdStatusList[i].Name}");
								}
							}
							else if(k == 1)
                            {
								if(cmdName == "Say")
                                {
									cmdBase.SetCSVContent1(dataList[i][k]);
								}
                            }
							else if(k == 2)
                            {
								if (cmdName == "Say")
								{
									cmdBase.SetCSVContent2(dataList[i][k]);
								}
							}
						}
	
					}
				}
				count += 4;
			}
			

			/*for (int i = 0; i < columnCount; i++)
			{
				if (i == 0) continue;
				for(int k = 0; k < rowCount; k++)
				{
					int count = 0;
					if(i == 1)
                    {
						if (k % rowCount + 1 != 0) return;
						var executorName = dataList[i][k];
						if (executorName != executors[count].name)
                        {
							Debug.LogWarning("フローチャートの名前が合いませんでした！ " + executorName);
                        }
						count++;
					}
					else
                    {
						var cmdlist = executor.Flowchart.GetReadOnlyCommandDataList();
					}
				}
			}*/
		}

		/// <summary>
		/// 絶対パスから Assets/ パスに変換する
		/// </summary>
		public static string AbsoluteToAssetsPath(this string self)
		{
			return self.Replace("\\", "/").Replace(Application.dataPath, "Assets");
		}

	}
	
	static class StringBuilderExtension
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