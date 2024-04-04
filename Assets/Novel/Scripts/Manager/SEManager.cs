using UnityEngine;

namespace Novel
{
    // ���j���[���b���̃N���b�N����炵�܂�
    // �e����SEManager�ɒu�������邱�Ƃ𐄏����܂�
    public class SEManager : SingletonMonoBehaviour<SEManager>
    {
        [SerializeField] AudioSource audioSource;

        public void PlaySE(AudioClip se, float volumeRate = 1f)
        {
            float volume = volumeRate;
            audioSource.PlayOneShot(se, volume);
        }
    }
}
