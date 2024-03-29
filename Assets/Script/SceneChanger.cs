using UnityEngine;
using UnityEngine.EventSystems;

namespace Novel
{
    /// <summary>
    /// �T�v: �V�[�����ړ����܂��B�{�^���̃C�x���g�ɐݒ肵���肵�Ďg���܂�
    /// �@�@  �܂��A�L�[��ݒ肷��ƁA���̃L�[�������ꂽ�ۂɃV�[�����ړ����܂�
    /// </summary>
    public class SceneChanger : MonoBehaviour
    {
        [SerializeField] string sceneName = "TitleScene";
        [SerializeField] float fadeTime = 0.5f;

        public void LoadScene()
        {
            EventSystem.current.SetSelectedGameObject(null);
            FadeLoadSceneManager.Instance.LoadScene(fadeTime, sceneName);
        }
    }
}