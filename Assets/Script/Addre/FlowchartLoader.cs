using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Novel
{
    public class FlowchartLoader : MonoBehaviour
    {
        [SerializeField] AssetReferenceT<FlowchartData> flowchartReference;
        [SerializeField] bool executeOnStart;

        void Start()
        {
            if (executeOnStart)
                ExecuteAsync().Forget();
        }

        public async UniTask ExecuteAsync()
        {
            var flowchart = await Addressables.LoadAssetAsync<FlowchartData>(flowchartReference);
            await flowchart.ExecuteAsync();
            Addressables.Release(flowchart);
        }
    }
}
