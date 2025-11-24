using LoopLegacy.State;
using R3;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AddressableAssets;

namespace LoopLegacy.Manager
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        private AudioSource _sfxAudioSource;
        private AudioSource _bgmAudioSource;
        private float _masterVolume;
        private float _bgmVolume;
        private float _sfxVolume;
        private Tween _bgmTween;
        
        // 배경음악 재생 위치 저장용 변수들
        private AudioClip _savedBGMClip;
        private float _savedBGMPosition;
        private bool _savedBGMLoop;
        
        void Awake()
        {
            Instance = this;
            _sfxAudioSource = gameObject.AddComponent<AudioSource>();
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();
            _sfxAudioSource.dopplerLevel = 0;
            _sfxAudioSource.reverbZoneMix = 0;
            _bgmAudioSource.dopplerLevel = 0;
            _bgmAudioSource.reverbZoneMix = 0;
        }

        void Start()
        {
            var d = Disposable.CreateBuilder();
            OptionState.Instance.MasterVolume.Subscribe(volume =>
            {
                _masterVolume = volume / 10.0f;
                _sfxAudioSource.volume = _masterVolume * _sfxVolume;
                _bgmAudioSource.volume = _masterVolume * _bgmVolume;
            }).AddTo(ref d);

            OptionState.Instance.SfxVolume.Subscribe(volume =>
            {
                _sfxVolume = volume / 10.0f;
                _sfxAudioSource.volume = _masterVolume * _sfxVolume;
            }).AddTo(ref d);

            OptionState.Instance.BgmVolume.Subscribe(volume =>
            {
                _bgmVolume = volume / 10.0f;
                _bgmAudioSource.volume = _masterVolume * _bgmVolume;
            }).AddTo(ref d);
            d.RegisterTo(this.destroyCancellationToken);
        }

        public void PlayBGM(AudioClip clip, bool loop)
        {
            if (_bgmAudioSource.clip == clip) return;

            _bgmAudioSource.clip = clip;
            _bgmAudioSource.Play();
            _bgmAudioSource.loop = loop;
        }

        public void StopBGM()
        {
            _bgmAudioSource.Stop();
        }

        // 현재 배경음악 상태를 저장
        public void SaveBGMState()
        {
            if (_bgmAudioSource.clip == null)
            {
                _savedBGMClip = null;
                _savedBGMPosition = 0;
            }
            if (_bgmAudioSource.isPlaying)
            {
                _savedBGMClip = _bgmAudioSource.clip;
                _savedBGMPosition = _bgmAudioSource.time;
                _savedBGMLoop = _bgmAudioSource.loop;
            }
        }

        // 저장된 배경음악 상태를 복원
        public void RestoreBGMState()
        {
            if (_savedBGMClip == null)
            {
                _bgmAudioSource.Stop();
                _bgmAudioSource.clip = null;
            }
            else
            {
                _bgmAudioSource.clip = _savedBGMClip;
                _bgmAudioSource.time = _savedBGMPosition;
                _bgmAudioSource.loop = _savedBGMLoop;
                _bgmAudioSource.Play();
            }
        }

        public void PlaySFXSound(AudioClip clip)
        {
            _sfxAudioSource.PlayOneShot(clip);
        }
    }
}
