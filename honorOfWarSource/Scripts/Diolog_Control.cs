using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Diolog_Control : MonoBehaviour {
    [Header("Audio Clips")]
    [SerializeField] AudioClip[] clip;
    [SerializeField] AudioSource GunSource;
    [SerializeField] AudioSource Music;
    private AudioSource audioSource;

    [Header("Fadeing settings")]
    [SerializeField] float durationIn;

    [Header("Dialog Boxes")]
    [SerializeField] GameObject[] dialogBox;

    private bool played;
    private bool end;
    private int count = 0;

    void Start() {
        audioSource = this.gameObject.GetComponent<AudioSource>();
        audioSource.volume = volControler.targetVolumeControl;
        GunSource.volume = volControler.targetVolumeControl;
        
        StartCoroutine(gunSoundCoroutine());
        StartCoroutine(startMusicCoroutine());
        audioSource.PlayOneShot(clip[0]);
    }

    void Update() {
        if(!audioSource.isPlaying){
            if((Input.GetButtonDown ("Fire1") || Input.GetKeyDown(KeyCode.Space)) && !end){
                dialogBox[count].SetActive(false);
                dialogBox[count + 1].SetActive(true);
                count++;
            }
            
            if(count == 3){
                if(!played){
                    audioSource.PlayOneShot(clip[1]);
                    played = true;
                }
            }

            if(count == dialogBox.Length - 1){
                if(!end)
                    StartCoroutine(loadSceneCoroutine());
                
                end = true;
            }
        }

        if(Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene("GameScene");
    }

    IEnumerator gunSoundCoroutine() {
        yield return new WaitForSeconds(5);
        GunSource.Play();
    }

    IEnumerator startMusicCoroutine() {
        yield return new WaitForSeconds(25);
        StartCoroutine(FadeAudioSource.StartFade(Music, durationIn, volControler.targetVolumeControl));
        Music.Play();
    }

    IEnumerator loadSceneCoroutine() {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("GameScene");
    }
}
