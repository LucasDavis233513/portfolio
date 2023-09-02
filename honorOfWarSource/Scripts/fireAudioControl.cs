using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireAudioControl : MonoBehaviour {
    [Header("Fadeing settings")]
    [SerializeField] float durationIn;
    private AudioSource firesource;
    
    void Start() {
        firesource = this.gameObject.GetComponent<AudioSource>();
        StartCoroutine(FadeAudioSource.StartFade(firesource, durationIn, volControler.targetVolumeControl));
    }
}
