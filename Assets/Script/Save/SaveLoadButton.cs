using UnityEngine;
using UnityEngine.UI;

public class SaveLoadButton : MonoBehaviour
{
    [field: SerializeField] public Button Button;

    public void SetStatus(SaveButtonStatus buttonStatus)
    {

    }

    public class SaveButtonStatus
    {
        public string Time;
        public string Name;
    }
}
