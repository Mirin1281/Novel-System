using UnityEngine;
using Novel;
using Cysharp.Threading.Tasks;

public class TestMoveScene : MonoBehaviour
{
    [SerializeField] FlowchartExecutor flowchart;

    async UniTask Start()
    {
        flowchart.Execute();
        await Wait.Seconds(1f, default);
        Debug.Log(Flowchart.CurrentExecutingFlowcharts.Count);
    }
}
