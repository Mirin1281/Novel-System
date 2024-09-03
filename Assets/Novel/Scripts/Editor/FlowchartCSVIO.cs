using Novel.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Novel.Editor
{
    public static class FlowchartCSVIO
    {
		public enum FlowchartType
        {
			None,
			Executor,
			Data,
        }

		public static async UniTask ExportFlowchartCommandDataAsync(FlowchartType type)
        {
			await UniTask.Yield();

			CSVIOSettingsData settingsData = GetSettingsData();
			var exportName = GetIOName(type);
			var sheetName = FlowchartEditorUtility.GetFileName(
				settingsData.CSVFolderPath, $"{exportName}_{settingsData.ExportFileName}", "csv");
			StreamWriter sw = new StreamWriter($"{settingsData.CSVFolderPath}/{sheetName}",
				false, Encoding.GetEncoding("shift_jis"));

			try
            {
				// 1行目は名前
				sw.Write("Name:,");
				sw.WriteLine($"{exportName},");

				// 2行目は空白
				sw.WriteLine();

				List<IFlowchartObject> chartObjs = GetSortedFlowchartObjects(type, settingsData.FlowchartFindMode);
				int maxFlowchartsCmdIndex = chartObjs.Max(f => f.Flowchart.GetReadOnlyCommandDataList().Count);

				// 3行目は各フローチャートの名前とディスクリプション
				var sb = new StringBuilder();
				foreach (var chart in chartObjs)
                {
					sb.AddCell(chart.Name)
						.AddCell(chart.Flowchart.Description)
						.Skip(settingsData.RowCount - 2);
				}
				sw.WriteLine(sb.ToString());
				sb.Clear();

				// 3行目は空白
				sw.WriteLine();

				// 4行目以降はコマンドデータ
				for (int i = 0; i < maxFlowchartsCmdIndex; i++)
				{
					foreach (var chart in chartObjs)
					{
						var list = chart.Flowchart.GetReadOnlyCommandDataList();
						if (i >= list.Count)
                        {
							sb.Skip(settingsData.RowCount);
							continue;
						}
						var cmdBase = list[i].GetCommandBase();

						// コマンド名
						sb.AddCell(GetCommandName(cmdBase));

						// コンテント1
						var content1 = cmdBase?.CSVContent1;
						sb.AddCell(content1);

						// コンテント2
						var content2 = cmdBase?.CSVContent2;
						sb.AddCell(content2);

						sb.Skip(settingsData.RowCount - 3);
					}

					sw.WriteLine(sb.ToString());
					sb.Clear();
				}

				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				var path = FlowchartEditorUtility.AbsoluteToAssetsPath(settingsData.CSVFolderPath);
				var csv = AssetDatabase.LoadAssetAtPath<TextAsset>($"{path}/{sheetName}");
				EditorGUIUtility.PingObject(csv);
				Debug.Log("<color=lightblue>CSVを書き出しました！</color>");
			}
			finally
            {
				sw.Flush();
				sw.Close();
			}
		}

		public static async UniTask ImportFlowchartCommandDataAsync(FlowchartType type)
		{
			await UniTask.Yield();
			CSVIOSettingsData settingsData = GetSettingsData();
			var dataList = LoadCSV();
			if (dataList == null) return;

			// 名前のチェック 
			var importName = GetIOName(type);
			var csvName = dataList[0][1];
			if (importName != csvName)
			{
				Debug.LogError("名前が一致しません！");
				Debug.LogError($"現在の名前: {importName}, CSVの名前: {csvName}");
				return;
			}
			dataList.RemoveAt(0); // もういらないのでこの行は削除する
			dataList.RemoveAt(0); // もういらないので次の行も削除する

			var chartObjs = GetSortedFlowchartObjects(type, settingsData.FlowchartFindMode);

			var dataFlowchartCount = Mathf.RoundToInt((dataList[0].Length + 1) / settingsData.RowCount);
			for (int i = 0; i < dataFlowchartCount; i++)
            {
				var startX = i * settingsData.RowCount;
				var csvExecutorName = dataList[0][startX];
				var meetFlowchart = chartObjs.Where(flowchart => flowchart.Name == csvExecutorName).FirstOrDefault();
				if (meetFlowchart == null)
                {
					Debug.LogWarning($"{csvExecutorName}という名前のフローチャートがシーン上に存在しませんでした");
					continue;
                }
				Import(meetFlowchart, dataList, startX);

				// セーブ
				if (type == FlowchartType.Executor)
				{
					var chartExecutor = meetFlowchart as FlowchartExecutor;
					foreach (var cmdData in chartExecutor.Flowchart.GetCommandDataList())
					{
						EditorUtility.SetDirty(cmdData);
					}
					EditorUtility.SetDirty(chartExecutor);
				}
				else if (type == FlowchartType.Data)
				{
					var chartData = meetFlowchart as FlowchartData;
					foreach (var cmdData in chartData.Flowchart.GetCommandDataList())
					{
						EditorUtility.SetDirty(cmdData);
					}
					EditorUtility.SetDirty(chartData);
				}
				AssetDatabase.SaveAssets();
			}
			Debug.Log("<color=lightblue>CSVを読み込みました！</color>");


			// フォルダメニューを開き、CSVファイルを読み込みます
			List<string[]> LoadCSV()
            {
				var absolutePath = EditorUtility.OpenFilePanel("Open csv", settingsData.CSVFolderPath, "csv");
				if (string.IsNullOrEmpty(absolutePath)) return null;
				var relativePath = FlowchartEditorUtility.AbsoluteToAssetsPath(absolutePath);

				var fs = new FileStream(relativePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				var encoding = Encoding.GetEncoding("shift_jis");
				var reader = new StreamReader(fs, encoding);
				var dataList = CSV2StringTable(reader);
				reader.Close();
				return dataList;


				// https://resanaplaza.com/2020/09/28/%E3%80%90c%E3%80%91csv%E3%81%AE%E8%AA%AD%E3%81%BF%E8%BE%BC%E3%81%BF%E3%83%AD%E3%82%B8%E3%83%83%E3%82%AF%E3%82%92%E7%B0%A1%E5%8D%98%E8%A7%A3%E8%AA%AC%EF%BC%88%E9%A0%85%E7%9B%AE%E4%B8%AD%E3%81%AE/
				static List<string[]> CSV2StringTable(StreamReader reader, char delimiter = ',')
				{
					List<string[]> result = new();
					List<string> line = new();
					StringBuilder cell = new StringBuilder();
					bool dq_flg = false; // ダブルクォーテーション中のフラグ

					while (reader.Peek() != -1)
					{
						char c = (char)reader.Read(); // 1文字読み込む

						// ダブルクオーテーションが見つかるとフラグを反転する
						dq_flg = (c == '\"') ? !dq_flg : dq_flg;

						// ダブルクォーテーション中ではないキャリッジリターンは破棄する
						if (c == '\r' && dq_flg == false)
						{
							continue;
						}

						// ダブルクォーテーション中ではない時にカンマが見つかったら、
						// それまでに読み取った文字列を１つのかたまりとしてlineに追加する
						if (c == delimiter && dq_flg == false)
						{
							line.Add(to_str(cell));
							cell.Clear();
							continue;
						}

						// ダブルクォーテーション中ではない時にラインフィードが見つかったら
						// 1行分のlineをresultに追加する
						if (c == '\n' && dq_flg == false)
						{
							line.Add(to_str(cell));
							result.Add(line.ToArray());
							line.Clear();
							cell.Clear();
							continue;
						}
						cell.Append(c);
					}

					// ファイル末尾が改行コードでない場合、ループを抜けてしまうので、
					// 未処理の項目がある場合は、ここでlineに追加
					if (cell.Length > 0)
					{
						line.Add(to_str(cell));
						result.Add(line.ToArray());
					}

					// デバッグ用
					/*for(int i = 0; i < result.Count; i++)
                    {
						for(int k = 0; k < result[i].Length; k++)
                        {
							if (string.IsNullOrEmpty(result[i][k])) continue;
							Debug.Log(result[i][k]);
                        }
                    }*/

				return result;
					

					// 前後のダブルクォーテーションを削除し、2個連続するダブルクォーテーションを1個に置換する
					static string to_str(StringBuilder p_str)
					{
						string l_val = p_str.ToString().Replace("\"\"", "\"");
						int l_start = l_val.StartsWith("\"") ? 1 : 0;
						int l_end = l_val.EndsWith("\"") ? 1 : 0;
						return l_val.Substring(l_start, l_val.Length - l_start - l_end);
					}
				}
			}

			// CSVの各情報をフローチャートに反映します
			void Import(IFlowchartObject chart, List<string[]> csvList, int startX)
			{
				var copiedCsvList = new List<string[]>(csvList);
				copiedCsvList.RemoveAt(0);
				copiedCsvList.RemoveAt(0);
				var columnCount = copiedCsvList.Count;
				// 行(横)をスライド
				for (int i = 0; i < columnCount; i++)
				{
					CommandBase colomn_cmdBase = null;
					var colomn_array = copiedCsvList[i];
					// 列(縦)をスライド
					for (int k = startX; k < startX + settingsData.RowCount; k++)
					{
						// コマンドの名前を見る
						if (k == startX)
						{
							string cellName = colomn_array[k];
							bool notExistCell = string.IsNullOrEmpty(cellName);
							
							var list = chart.Flowchart.GetCommandDataList();
							if (list == null) list = new();

							// CSVにコマンドがあるのにフローチャートにはない場合、名前から新しくつくる
							if (i >= list.Count)
							{
								if (notExistCell) break;
								var newCmdData = type switch
								{
									FlowchartType.Executor => ScriptableObject.CreateInstance<CommandData>(),
									FlowchartType.Data => FlowchartEditorUtility.CreateCommandData(
														ConstContainer.COMMANDDATA_PATH, $"{nameof(CommandData)}_{chart.Name}"),
									_ => throw new Exception(),
								};
								if (cellName is not ("<Null>" or "Null"))
								{
									Type type = GetTypeByClassName(cellName);
									if (type == null) break;
									newCmdData.SetCommand(type);
									colomn_cmdBase = newCmdData.GetCommandBase();
								}
								list.Add(newCmdData);
							}
							else // コマンドがある場合、名前が一致してるか調べる
							{
								var cmdData = list[i];
								colomn_cmdBase = cmdData.GetCommandBase();
								var cmdName = GetCommandName(colomn_cmdBase);

								if (cmdName == cellName) continue;
								if (settingsData.IsChangeIfDifferentCmdName && (cellName != "Null" || cellName != "<Null>"))
                                {
									Debug.LogWarning(
										$"コマンドの名前が合いませんので上書きします\n" +
										$"Before: {cmdName}, After: {cellName}");
								}
								else
                                {
									Debug.LogWarning(
										$"コマンドの名前が合いませんのでスキップされました\n" +
										$"Before: {cmdName}, After: {cellName}");
									break;
								}

								if (notExistCell == false && cellName is not ("<Null>" or "Null"))
                                {
									Type type = GetTypeByClassName(cellName);
									if (type == null) break;
									cmdData.SetCommand(type);
									colomn_cmdBase = cmdData.GetCommandBase();
								}
								else
                                {
									cmdData.SetCommand(null);
								}
							}
						}
						else if (k == startX + 1 && colomn_cmdBase != null)
						{
							colomn_cmdBase.CSVContent1 = colomn_array[k];
						}
						else if (k == startX + 2 && colomn_cmdBase != null)
						{
							colomn_cmdBase.CSVContent2 = colomn_array[k];
						}
					}
				}
			}
		}

		static string GetIOName(FlowchartType type)
        {
			return type switch
			{
				FlowchartType.Executor => SceneManager.GetActiveScene().name,
				FlowchartType.Data => "ScriptableObjects",
				_ => throw new Exception()
			};
		}

		static CSVIOSettingsData GetSettingsData()
		{
			var settingsData = FlowchartEditorUtility.GetAllScriptableObjects<CSVIOSettingsData>().FirstOrDefault();
			if (settingsData == null)
			{
				Debug.LogWarning($"{nameof(CSVIOSettingsData)}が見つかりませんでした");
				throw new FileNotFoundException();
			}
			else
			{
				return settingsData;
			}
		}

		static string GetCommandName(CommandBase commandBase)
		{
			if (commandBase == null) return "Null";
			return commandBase.ToString()
				.Replace($"{nameof(Novel)}.{nameof(Command)}.", string.Empty);
		}

		static Type GetTypeByClassName(string className)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{
					if (type.Name == className && 
						type.Namespace == "Novel.Command")
					{
						return type;
					}
				}
			}
			Debug.LogWarning($"{className}クラスが見つかりませんでした！\n" +
				$"タイポもしくは{className}クラスが名前空間\"Novel.Command\"内に存在しない可能性があります");
			return null;
		}

		static List<IFlowchartObject> GetSortedFlowchartObjects(FlowchartType type, FindObjectsInactive findMode)
        {
			return type switch
			{
				FlowchartType.Executor => GetFlowchartObjectsByHierarchy(),
				FlowchartType.Data => FlowchartEditorUtility.GetAllScriptableObjects<FlowchartData>()
					.OrderBy(f => f.Name)
					.Select(f => f as IFlowchartObject)
					.ToList(),
				_ => throw new Exception(),
			};


			static List<IFlowchartObject> GetFlowchartObjectsByHierarchy()
			{
				List<GameObject> allObjects = new();
				GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

				foreach (var obj in rootObjects)
				{
					allObjects.Add(obj);
					AddAllChildren(obj, allObjects);
				}

				return allObjects.Select(o => o.GetComponent<IFlowchartObject>())
					.Where(f => f != null).ToList();


				static void AddAllChildren(GameObject parent, List<GameObject> list)
				{
					foreach (Transform child in parent.transform)
					{
						list.Add(child.gameObject);
						AddAllChildren(child.gameObject, list); // 再帰呼び出しで子オブジェクトを追加
					}
				}
			}
		}
	}
	
	static class StringBuilderExtension
    {
		public static StringBuilder Skip(this StringBuilder sb, int count)
        {
			for(int i = 0; i < count; i++)
            {
				sb.Append(",");
			}
			return sb;
		}

		public static StringBuilder AddCell(this StringBuilder sb, string content)
		{
			if (content != null && (content.Contains("\"") || content.Contains(",") || content.Contains("\n")))
			{
				content = content.Replace("\"", "\"\"");
				sb.Append("\"").Append(content).Append("\"").Append(",");
			}
			else
			{
				sb.Append(content).Append(",");
			}
			return sb;
		}
	}
}