using System.Collections.Generic;
using UnityEngine;

namespace Manager.Sound
{
    public class SoundPlayer : MonoBehaviour
    {
        public static SoundPlayer instance;

        // 오디오 클립을 저장할 변수
        public AudioClip[] clips;
        public Dictionary<string, AudioClip> clipsDictionary;

        // 오디오 플레이 변수
        AudioSource sfxPlayer;
        AudioSource bgmPlayer;

        private float sfxVolume = 1f;
        private float bgmVolume = 1f;

        void Awake()
        {
            instance = GetComponent<SoundPlayer>();

            sfxPlayer = GameObject.Find("SFXSound").GetComponent<AudioSource>();
            bgmPlayer = GameObject.Find("BGMSound").GetComponent<AudioSource>();

            clipsDictionary = new Dictionary<string, AudioClip>();
            foreach (AudioClip clip in clips)
            {
                clipsDictionary.Add(clip.name, clip);
            }
        }

        public void PlaySound(string clipname, float volume = 1f)
        {
            if (clipsDictionary.ContainsKey(clipname) == false)
            {
                return;
            }
            sfxPlayer.PlayOneShot(clipsDictionary[clipname], volume * sfxVolume);
        }

        public void PlayBGM(string clipname, float volume = 1f, bool loop = true)
        {
            if (clipsDictionary.ContainsKey(clipname) == false)
            {
                return;
            }

            bgmPlayer.clip = clipsDictionary[clipname];
            bgmPlayer.volume = sfxVolume;
            bgmPlayer.loop = loop;
            bgmPlayer.Play();
        }

        public void StopBGM()
        {
            bgmPlayer.Stop();
        }

        public void StopSFX()
        {
            sfxPlayer.Stop();
        }

        public void SetSFX(float volume)
        {
            sfxPlayer.volume = volume;
            sfxVolume = volume;
        }

        public void SetBGM(float volume)
        {
            bgmPlayer.volume = volume;
            bgmVolume = volume;
        }
    }
}
