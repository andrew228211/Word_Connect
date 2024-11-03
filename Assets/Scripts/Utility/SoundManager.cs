using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AT_InGame
{

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;

        public AudioSource originAS;
        public AudioClip[] bgClips;
        public AudioClip loseClip, btnClickClip;
        public AudioClip[] victoryClips;
        public AudioClip flyCoinClip;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            asPool.Add(originAS);
        }
        private void Start()
        {
            PlayRandomMusic();
        }
        public bool music
        {
            get { return PlayerPrefs.GetInt("music") == 0; }
            private set { PlayerPrefs.SetInt("music", value ? 0 : 2020); }
        }
        public bool sfx
        {
            get { return PlayerPrefs.GetInt("sfx") == 0; }
            private set { PlayerPrefs.SetInt("sfx", value ? 0 : 2020); }
        }
        public bool vibration
        {
            get { return PlayerPrefs.GetInt("vibration") == 0; }
            private set { PlayerPrefs.SetInt("vibration", value ? 0 : 2020); }
        }
        public AudioSource bgAS;


        void PlayRandomMusic()
        {
            if (music)
            {
                if (bgClips.Length > 0)
                {
                    StartCoroutine(playRndMusic());
                }
            }
        }
        IEnumerator playRndMusic()
        {
            AudioClip clip = bgClips[Random.Range(0, bgClips.Length)];
            //bgAS = GetAudioSource();
            bgAS.clip = clip;
            bgAS.volume = 0.5f;
            bgAS.Play();
            while (bgAS.isPlaying)
            {
                yield return null;
            }
            PlayRandomMusic();
        }
        public AudioSource PlaySfx(AudioClip clip, float delay = 0)
        {
            if (!sfx) return null;

            AudioSource audioS = GetAudioSource();
            audioS.clip = clip;
            audioS.time = 0;
            audioS.loop = false;
            audioS.volume = 1;
            if (delay > 0)
                audioS.PlayDelayed(delay);
            else
                audioS.Play();
            return audioS;
        }
        public void PlayBtnClick()
        {
            PlaySfx(btnClickClip);
        }
        public void PlayCoin()
        {
            PlaySfx(flyCoinClip); ;
        }
        public void PlayLose()
        {
            PlaySfx(loseClip);
        }
        public void PlayVictory()
        {
            PlayRndSfx(victoryClips);
        }
        public void PlayRndSfx(params AudioClip[] clips)
        {
            PlaySfx(clips[Random.Range(0, clips.Length)]);
        }
        [SerializeField]
        List<AudioSource> asPool = new List<AudioSource>();
        AudioSource GetAudioSource()
        {
            for (int i = 0; i < asPool.Count; i++)
            {
                if (!asPool[i].isPlaying)
                    return asPool[i];
            }

            AudioSource newAS = Instantiate(originAS, originAS.transform.parent);
            asPool.Add(newAS);
            return newAS;
        }

        public delegate void myFunc();
        public myFunc musicOnOff, sfxOnOff, vibrationOnOff;
        public void SetMusic(bool value)
        {
            if (music != value)
            {
                music = value;
                if (value)
                    PlayRandomMusic();
                else
                    bgAS.Stop();

                if (musicOnOff != null)
                    musicOnOff();
            }
        }
        public void SetSfx(bool value)
        {
            if (sfx != value)
            {
                sfx = value;
                if (sfxOnOff != null)
                    sfxOnOff();
            }
        }
        public void SetVibration(bool value)
        {
            if (vibration != value)
            {
                vibration = value;

                if (vibrationOnOff != null)
                    vibrationOnOff();
            }
        }
        public void Vibrate(int miliseconds)
        {
            if (!vibration) return;

            Vibration.Vibrate(miliseconds);
        }
    }
}
