using UnityEngine;

public class BeforeAwakeInit
{
	// この属性によりAwakeより前に処理が走る
	// ScriptableObjectからマネージャーを生成する
	// ここでしか呼ばないこと
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Init()
	{
		var createManagerData = Resources.Load("CreateManagerData") as CreateManagerData;
		createManagerData.InitCreate();
	}
}
