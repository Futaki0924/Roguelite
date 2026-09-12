using UnityEngine;

namespace Core.Manager
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("スピーカー設定")]
        [SerializeField] AudioSource _bgmSource;
        [SerializeField] AudioSource _seSource;

        [Header("音量設定")]
        [Range(0, 1f)] public float MasterVolume = 1f;
        [Range(0, 1f)] public float BGMVolume = 1f;
        [Range(0, 1f)] public float SEVolume = 1f;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayBGM(AudioClip clip)
        {
            if(clip != null)
            {
                _bgmSource.clip = clip;
                _bgmSource.volume = MasterVolume * BGMVolume;
                _bgmSource.Play();
            }
        }

        public void PlaySE(AudioClip clip)
        {
            if (clip != null)
            {
                _seSource.volume = MasterVolume * SEVolume;
                _seSource.PlayOneShot(clip);
            }
        }

        public void UpdateVolumes()
        {
            _bgmSource.volume = MasterVolume * BGMVolume;
            _seSource.volume = MasterVolume * SEVolume;
        }
    }
}