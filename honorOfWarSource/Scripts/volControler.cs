using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class volControler : MonoBehaviour {
    public static float targetVolumeControl;
    private Scene scene;
    private bool loaded;

    void Start() {
        if(!loaded)
            DontDestroyOnLoad(this.gameObject);
        else if(loaded && scene.name == "Main Menu") 
            Destroy(this.gameObject);
    }

    void Update() {
        scene = SceneManager.GetActiveScene();
        if(scene.name == "Gamescene"){
            Debug.Log("loaded scene");
            loaded = true;
        }
        
        Start();
    }
}
