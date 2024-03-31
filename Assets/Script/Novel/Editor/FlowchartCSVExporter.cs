using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Novel.Command;
using System;
using System.Reflection;

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
				sw.Write("SceneName:,");
				sw.WriteLine(sceneName);

				var executors = GetSortedFlowchartExecutors();

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
			var dataList = LoadCSV();
			if (dataList == null) return;
			int csvFlowchartCount = dataList[0].Length / (rowCount + 1);

			// シーン名のチェック
			var currentSceneName = SceneManager.GetActiveScene().name;
			var csvSceneName = dataList[0][1];
			if (currentSceneName != csvSceneName)
			{
				Debug.LogError("シーンの名前が一致しません！\n" +
					$"今のシーン名: {currentSceneName}, CSVのシーン名: {csvSceneName}");
				return;
			}
			// もういらないのでこの行は削除する
			dataList.RemoveAt(0);

			var executors = GetSortedFlowchartExecutors();

			// フローチャート名のチェック
			for (int i = 0; i < csvFlowchartCount; i++)
			{
				var executorObjectName = executors[i].name;
				var csvExecutorName = dataList[0][i * (rowCount + 1)];
				if (executorObjectName != csvExecutorName)
				{
					Debug.LogWarning("次のフローチャートの名前が合いません！\n" +
						$"Object: {executorObjectName}, CSV: {csvExecutorName}");
					return;
				}
			}
			dataList.RemoveAt(0);

			for (int i = 0; i < executors.Count; i++)
			{
				ImportByExecutor(executors[i], dataList, i * (rowCount + 1));
				EditorUtility.SetDirty(executors[i]);
			}
			Debug.Log("<color=lightblue>CSVをインポートしました</color>");


			// フォルダメニューを開き、CSVファイルを読み込みます
			static List<string[]> LoadCSV()
            {
				var absolutePath = EditorUtility.OpenFilePanel("Open csv", Application.dataPath, "csv");
				if (string.IsNullOrEmpty(absolutePath)) return null;
				var relativePath = FlowchartEditorUtility.AbsoluteToAssetsPath(absolutePath);

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
				return dataList;
			}

			static void ImportByExecutor(FlowchartExecutor executor, List<string[]> csvList, int startX)
			{
				var columnCount = csvList.Count;
				for (int i = 0; i < columnCount; i++)
				{
					string cmdName = null;
					CommandBase cmdBase = null;
					for (int k = startX; k < rowCount + startX; k++)
					{
						if (k == startX)
						{
							if (string.IsNullOrEmpty(csvList[i][k])) break;

							var list = executor.Flowchart.GetCommandDataList();
							if(list == null || list.Count == 0)
                            {
								Debug.Log("フローチャートがな～い");
								continue;
                            }

							var cellName = csvList[i][k];
							if (i >= list.Count)
                            {
								var newCmdData = ScriptableObject.CreateInstance<CommandData>();
								Type type = GetTypeByClassName($"{cellName}Command");
								if(type == null)
                                {
									Debug.Log($"{cellName}というコマンドはありません！クラス名で指定してください！");
									continue;
								}
								cmdName = cellName;
								newCmdData.SetCommand(type);
								cmdBase = newCmdData.GetCommandBase();
								list.Insert(i, newCmdData);
							}
							else
                            {
								var cmdData = list[i];
								if (cmdData == null) continue;
								cmdName = cmdData.GetCommandStatus().Name;
								cmdBase = cmdData.GetCommandBase();
								
								if (cmdName != cellName)
								{
									Debug.LogWarning($"コマンドの名前が合いません！\n" +
										$"Object: {cmdName}, CSV: {cellName}");
								}
							}
							
						}
						else if (k == startX + 1)
						{
							if (cmdName is "Say")
							{
								cmdBase.SetCSVContent1(csvList[i][k]);
							}
						}
						else if (k == startX + 2)
						{
							if (cmdName is "Say")
							{
								cmdBase.SetCSVContent2(csvList[i][k]);
							}
						}
					}
				}
			}
		}

		public static Type GetTypeByClassName(string className)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.Name == className)
					{
						return type;
					}
				}
			}
			return null;
		}

		public static List<FlowchartExecutor> GetSortedFlowchartExecutors()
			=> GameObject.FindObjectsByType<FlowchartExecutor>(
					FindObjectsInactive.Include, FindObjectsSortMode.None)
					.OrderBy(f => f.name)
					.ToList();
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