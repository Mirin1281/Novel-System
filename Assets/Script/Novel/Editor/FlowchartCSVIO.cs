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

namespace Novel
{
    public static class FlowchartCSVIO
    {
		readonly static string outPutFileName = "FlowchartSheet";
		readonly static string defaultPath = $"{Application.dataPath}/CSV";
		readonly static int rowCount = 3;
		readonly static bool isChangeIfDifferentCmdName = true;
		readonly static FindObjectsInactive findMode = FindObjectsInactive.Include;
		public enum FlowchartType
        {
			None,
			Executor,
			Data,
        }

		public static async UniTask ExportFlowchartCommandDataAsync(string exportName, FlowchartType type)
        {
			await UniTask.Yield();
			var name = FlowchartEditorUtility.GetFileName(
				defaultPath, $"{exportName}_{outPutFileName}", "csv");
			StreamWriter sw = new($"{defaultPath}/{name}", false, Encoding.GetEncoding("shift_jis"));

			try
            {
				// 1�s�ڂ͖��O
				sw.Write("\"Name:\",");
				sw.WriteLine($"\"{exportName}\",");

				List<IFlowchartObject> chartObjs = GetSortedFlowchartObjects(type);
				int maxFlowchartsCmdIndex = chartObjs.Max(f => f.Flowchart.GetReadOnlyCommandDataList().Count);

				// 2�s�ڂ͊e�t���[�`���[�g�̖��O
				var sb = new StringBuilder();
				foreach (var chart in chartObjs)
                {
					sb.Append("\"").Append(chart.Name).Append("\"").Append(",")
						.Skip(rowCount);
				}
				sw.WriteLine(sb.ToString());
				sb.Clear();

				for (int i = 0; i < maxFlowchartsCmdIndex; i++)
				{
					foreach (var chart in chartObjs)
					{
						var list = chart.Flowchart.GetReadOnlyCommandDataList();
						if (i >= list.Count)
                        {
							sb.Skip(rowCount + 1);
							continue;
						}
						var cmdBase = list[i].GetCommandBase();

						var content1 = cmdBase?.CSVContent1;
						if(content1 != null && content1.Contains("\""))
                        {
							content1 = content1.Replace("\"", "\"\"");
						}
						var content2 = cmdBase?.CSVContent2;
						if (content2 != null && content2.Contains("\""))
						{
							content2 = content2.Replace("\"", "\"\"");
						}

						sb.Append("\"").Append(GetCommandName(cmdBase)).Append("\"").Append(",")
							.Append("\"").Append(content1).Append("\"").Append(",")
							.Append("\"").Append(content2).Append("\"").Append(",")
							.Skip(1);
					}

					sw.WriteLine(sb.ToString());
					sb.Clear();
				}

				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				var path = FlowchartEditorUtility.AbsoluteToAssetsPath(defaultPath);
				var csv = AssetDatabase.LoadAssetAtPath<TextAsset>($"{path}/{name}");
				EditorGUIUtility.PingObject(csv);
				Debug.Log("<color=lightblue>CSV�������o���܂����I</color>");
			}
			finally
            {
				sw.Flush();
				sw.Close();
			}
		}


		enum ImportType
		{
			None,
			New,
			Change,
			Override,
		}

		public static async UniTask ImportFlowchartCommandDataAsync(string importName, FlowchartType type)
		{
			await UniTask.Yield();
			var dataList = LoadCSV();
			if (dataList == null) return;

			// ���O�̃`�F�b�N
			var csvName = dataList[0][1];
			if (importName != csvName)
			{
				Debug.LogError("���O����v���܂���I\n" +
					$"���݂̖��O: {importName}, CSV�̖��O: {csvName}");
				return;
			}
			dataList.RemoveAt(0); // ��������Ȃ��̂ł��̍s�͍폜����

			var chartObjs = GetSortedFlowchartObjects(type);

			// �t���[�`���[�g���̃`�F�b�N
			int j = 0;
			int k = 0;
			while(j < chartObjs.Count)
			{
				var executorObjectName = chartObjs[j].Name;
				if (k * (rowCount + 1) >= dataList[0].Length)
                {
					j++;
					continue;
				}
				var csvExecutorName = dataList[0][k * (rowCount + 1)];
				if (executorObjectName == csvExecutorName)
                {
					j++;
					k++;
                }
				else
				{
					chartObjs.Remove(chartObjs[j]);
					k++;
				}
			}
			dataList.RemoveAt(0);

			for (int i = 0; i < chartObjs.Count; i++)
			{
				ImportByExecutor(chartObjs[i], dataList, i * (rowCount + 1));

				if (type == FlowchartType.Executor)
				{
					var chartExecutor = chartObjs[i] as FlowchartExecutor;
					foreach (var cmdData in chartExecutor.Flowchart.GetCommandDataList())
					{
						EditorUtility.SetDirty(cmdData);
					}
					EditorUtility.SetDirty(chartExecutor);
				}
				else if (type == FlowchartType.Data)
                {
					var chartData = chartObjs[i] as FlowchartData;
					foreach (var cmdData in chartData.Flowchart.GetCommandDataList())
                    {
						EditorUtility.SetDirty(cmdData);
					}
					EditorUtility.SetDirty(chartData);
				}
				
				AssetDatabase.SaveAssets();
			}
			
			Debug.Log("<color=lightblue>CSV��ǂݍ��݂܂����I</color>");


			// �t�H���_���j���[���J���ACSV�t�@�C����ǂݍ��݂܂�
			static List<string[]> LoadCSV()
            {
				var absolutePath = EditorUtility.OpenFilePanel("Open csv", Application.dataPath, "csv");
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
					bool dq_flg = false; // �_�u���N�H�[�e�[�V�������̃t���O

					while (reader.Peek() != -1)
					{
						char c = (char)reader.Read(); // 1�����ǂݍ���

						// �_�u���N�I�[�e�[�V������������ƃt���O�𔽓]����
						dq_flg = (c == '\"') ? !dq_flg : dq_flg;

						// �_�u���N�H�[�e�[�V�������ł͂Ȃ��L�����b�W���^�[���͔j������
						if (c == '\r' && dq_flg == false)
						{
							continue;
						}

						// �_�u���N�H�[�e�[�V�������ł͂Ȃ����ɃJ���}������������A
						// ����܂łɓǂݎ������������P�̂����܂�Ƃ���line�ɒǉ�����
						if (c == delimiter && dq_flg == false)
						{
							line.Add(to_str(cell));
							cell.Clear();
							continue;
						}

						// �_�u���N�H�[�e�[�V�������ł͂Ȃ����Ƀ��C���t�B�[�h������������
						// 1�s����line��result�ɒǉ�����
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

					// �t�@�C�����������s�R�[�h�łȂ��ꍇ�A���[�v�𔲂��Ă��܂��̂ŁA
					// �������̍��ڂ�����ꍇ�́A������line�ɒǉ�
					if (cell.Length > 0)
					{
						line.Add(to_str(cell));
						result.Add(line.ToArray());
					}

					// �f�o�b�O�p
					/*for(int i = 0; i < result.Count; i++)
                    {
						for(int k = 0; k < result[i].Length; k++)
                        {
							if (string.IsNullOrEmpty(result[i][k])) continue;
							Debug.Log(result[i][k]);
                        }
                    }*/

					return result;
					

					// �O��̃_�u���N�H�[�e�[�V�������폜���A2�A������_�u���N�H�[�e�[�V������1�ɒu������
					static string to_str(StringBuilder p_str)
					{
						string l_val = p_str.ToString().Replace("\"\"", "\"");
						int l_start = (l_val.StartsWith("\"")) ? 1 : 0;
						int l_end = l_val.EndsWith("\"") ? 1 : 0;
						return l_val.Substring(l_start, l_val.Length - l_start - l_end);
					}
				}
			}

			// CSV�̊e�����t���[�`���[�g�ɔ��f���܂�
			void ImportByExecutor(IFlowchartObject chart, List<string[]> csvList, int startX)
			{
				var columnCount = csvList.Count;
				// �s(��)���X���C�h
				for (int i = 0; i < columnCount; i++)
				{
					CommandBase colomn_cmdBase = null;
					ImportType colomn_importType = ImportType.None;
					var colomn_array = csvList[i];
					// ��(�c)���X���C�h
					for (int k = startX; k < rowCount + startX; k++)
					{
						// �R�}���h�̖��O������
						if (k == startX)
						{
							var cellName = colomn_array[k];
							bool existCell = string.IsNullOrEmpty(cellName) == false;
							
							var list = chart.Flowchart.GetCommandDataList();
							if (list == null) list = new();

							// CSV�ɃR�}���h������̂Ƀt���[�`���[�g�ɂ͂Ȃ��ꍇ�A���O����V��������
							if (i >= list.Count)
							{
								if (existCell == false) break;
								colomn_importType = ImportType.New;
								var newCmdData = type switch
								{
									FlowchartType.Executor => ScriptableObject.CreateInstance<CommandData>(),
									FlowchartType.Data => FlowchartEditorUtility.CreateCommandData(
														NameContainer.COMMANDDATA_PATH, $"CommandData_{chart.Name}"),
									_ => throw new Exception(),
								};
								if (cellName is not "<Null>")
								{
									Type type = GetTypeByClassName($"{cellName}Command");
									if (type == null) break;
									newCmdData.SetCommand(type);
									colomn_cmdBase = newCmdData.GetCommandBase();
								}
								list.Insert(i, newCmdData);
							}
							else // �R�}���h������ꍇ�A���O����v���Ă邩���ׂ�
							{
								colomn_importType = ImportType.Override;
								var cmdData = list[i];
								colomn_cmdBase = cmdData.GetCommandBase();
								var cmdName = GetCommandName(colomn_cmdBase);

								if (cmdName == cellName) continue;
								if (isChangeIfDifferentCmdName)
                                {
									Debug.LogWarning(
										$"�R�}���h�̖��O�������܂���̂ŏ㏑������܂���\n" +
										$"Object: {cmdName}, CSV: {cellName}");
								}
								else
                                {
									Debug.LogWarning(
										$"�R�}���h�̖��O�������܂���̂ŃX�L�b�v����܂���\n" +
										$"Object: {cmdName}, CSV: {cellName}");
									break;
								}

								colomn_importType = ImportType.Change;
								if (existCell && cellName != "<Null>")
                                {
									Type type = GetTypeByClassName($"{cellName}Command");
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
						else if (k == startX + 1 &&
							colomn_importType != ImportType.None &&
							colomn_cmdBase != null)
						{
							colomn_cmdBase.CSVContent1 = colomn_array[k];
						}
						else if (k == startX + 2 &&
							colomn_importType != ImportType.None &&
							colomn_cmdBase != null)
						{
							colomn_cmdBase.CSVContent2 = colomn_array[k];
						}
					}
				}
			}
		}

		static string GetCommandName(CommandBase commandBase)
		{
			if (commandBase == null) return "<Null>";
			return commandBase.ToString()
				.Replace("Novel.Command.", string.Empty)
				.Replace("Command", string.Empty);
		}

		static Type GetTypeByClassName(string className)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.Name == className) return type;
				}
			}
			Debug.Log($"{className}�N���X��������܂���ł����I");
			return null;
		}

		static List<IFlowchartObject> GetSortedFlowchartObjects(FlowchartType type)
        {
			return type switch
			{
				FlowchartType.Executor => Object.FindObjectsByType<FlowchartExecutor>(
											findMode, FindObjectsSortMode.None)
											.OrderBy(f => f.name)
											.Select(f => f as IFlowchartObject)
											.ToList(),
				FlowchartType.Data => FlowchartEditorUtility.GetAllScriptableObjects<FlowchartData>()
											.OrderBy(f => f.name)
											.Select(f => f as IFlowchartObject)
											.ToList(),
				_ => throw new Exception(),
			};
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