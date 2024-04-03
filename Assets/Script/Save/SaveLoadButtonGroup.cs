using Cysharp.Threading.Tasks;
using Novel;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SaveLoadType
{
    None,
    Save,
    Load,
}
public class SaveLoadButtonGroup : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] AudioClip selectSE;
    SaveLoadButton[] buttons;
    bool isFirstShow = true;

    void Awake()
    {
        canvas.gameObject.SetActive(false);
        buttons = GetComponentsInChildren<SaveLoadButton>();
    }

    public async UniTask ShowAndWaitButtonClick(SaveLoadType type, CancellationToken token)
    {
        // ���߂ĕ\������Ƃ��A�S�����[�h���ăX�e�[�^�X��ς���(�d���̂Ŕ񓯊��̕�����������)
        if(isFirstShow)
        {
            SetAllText();
            isFirstShow = false;
        }
        canvas.gameObject.SetActive(true);
        var tasks = new UniTask[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            tasks[i] = buttons[i].Button.OnClickAsync(token);
        }
        EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        int clickIndex = await UniTask.WhenAny(tasks);
        canvas.gameObject.SetActive(false);
        if (selectSE != null)
        {
            SEManager.Instance.PlaySE(selectSE);
        }

        if(type == SaveLoadType.Save)
        {
            // �Z�[�u���A�����ꂽ�{�^���̃X�e�[�^�X��ύX
            var gameData = Save(clickIndex);
            var buttonStatus = new SaveLoadButton.SaveButtonStatus()
            {
                Time = "",//gameData.Time,
                Name = "",//gameData.Name,
            };
            buttons[clickIndex].SetStatus(buttonStatus);
        }
        else if(type == SaveLoadType.Load)
        {
            Load(clickIndex);
        }
    }

    void SetAllText()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            var gameData = SaveLoad.LoadDataImmediate(i);
            var buttonStatus = new SaveLoadButton.SaveButtonStatus()
            {
                Time = "",//gameData.Time,
                Name = "",//gameData.Name,
            };
            buttons[i].SetStatus(buttonStatus);
        }
    }

    // �e���Ŏ�������
    GameData Save(int saveIndex)
    {
        var gameData = SaveLoad.LoadDataImmediate(saveIndex);
        var savables = this.FindObjectOfInterfaces<ISavable>();
        savables.ForEach(s => s.Save(gameData));
        SaveLoad.SaveDataImmediate(gameData, saveIndex);
        return gameData;
    }

    void Load(int saveIndex)
    {
        var gameData = SaveLoad.LoadDataImmediate(saveIndex);
        // �V�[���̃��[�h�Ƃ�
        // �t���[�`���[�g�Ƃ��̌����Ƃ�
    }
}