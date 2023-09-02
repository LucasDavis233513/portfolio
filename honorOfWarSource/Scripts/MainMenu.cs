using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    [Header("Fadeing settings")]
    [SerializeField] float durationIn;
    private float targetVolume;

    [Header("Audio Source")]
    [SerializeField] AudioSource audioSource;

    [Header("Volume control")]
    public Slider volumeControl;

    void Start() {
        targetVolume = volumeControl.value * 0.5f;
        StartCoroutine(FadeAudioSource.StartFade(audioSource, durationIn, targetVolume));
        audioSource.Play();
    }

    void Update() {
        volControler.targetVolumeControl = targetVolume;
        targetVolume = volumeControl.value * 0.5f;
        audioSource.volume = targetVolume;
    }

    public void PlayGame(){
        SceneManager.LoadScene("Dialog");
    }

    public void QuitGame() {
        Debug.Log("QUIT");
        Application.Quit();
    }
}
