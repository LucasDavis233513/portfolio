using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Sound.MusicPlayer {
    [RequireComponent(typeof (AudioSource))]

    public class MusicPlayer : MonoBehaviour {
        [Header("Enemy Attention")]
        public bool enemy = false;
        public bool playOnce;

        [Header("Audio Clips")]
        [SerializeField] AudioClip[] clips;
        [SerializeField] AudioClip CombatClip;
        [SerializeField] AudioSource windSource;
        [SerializeField] AudioSource birdSource;
        private AudioSource audioSource;

        [Header("Fadeing settings")]
        [SerializeField] float durationIn;

        [Header("FadeOut settings")]
        [SerializeField] float durationOut;
        [SerializeField] float targetVolumeOut;

        // Start is called before the first frame update
        void Start() {
            audioSource = this.gameObject.GetComponent<AudioSource>();
            audioSource.volume = 0;
            windSource.volume = volControler.targetVolumeControl;
            birdSource.volume = volControler.targetVolumeControl;

            audioSource.loop = false;
        }

        // Update is called once per frame
        void Update() {
            if((enemy != true) && !playOnce){
                StartCoroutine(FadeAudioSource.StartFade(audioSource, durationOut, targetVolumeOut));
                playOnce = true; 
            } else if((enemy == true) && !playOnce) {
                StartCoroutine(FadeAudioSource.StartFade(audioSource, durationOut, targetVolumeOut));
                playOnce = true; 
            }

            if(audioSource.volume == 0)
                audioSource.Stop();


            if(enemy != true) {
                if(!audioSource.isPlaying) {
                    audioSource.clip = GetRandomClip();
                    StartCoroutine(FadeAudioSource.StartFade(audioSource, durationIn, volControler.targetVolumeControl));
                    audioSource.Play();
                }
            } else { 
                if(!audioSource.isPlaying) {
                    audioSource.clip = CombatClip;
                    StartCoroutine(FadeAudioSource.StartFade(audioSource, durationIn, volControler.targetVolumeControl));
                    audioSource.Play();
                }
            }
        }

        private AudioClip GetRandomClip() {
            return clips[Random.Range(0, clips.Length)];
        }
    }
}
