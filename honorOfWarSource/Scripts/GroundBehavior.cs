using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.PlayerController;
using UnityEngine;

public class GroundBehavior : MonoBehaviour {
    [Header("Ground type and sound arrays")]
    public List<GroundType> GroundTypes = new List<GroundType>();

    [Header("PlayerInput Controler script")]
    public PlayerInput PC;
    
    private string currentground;

    void Start() {
        setGroundType (GroundTypes [0]);
    }

    void OnCollisionEnter(Collision hit) {
        if(hit.transform.tag == "Grass"){
            Debug.Log("Grass");
            setGroundType (GroundTypes[0]);
        }else if(hit.transform.tag == "Wood"){
            Debug.Log("Wood");
            setGroundType (GroundTypes[1]);
        }else{
            Debug.Log("rock");
            setGroundType (GroundTypes[2]);
        }
    }

    public void setGroundType(GroundType ground){
        if (currentground != ground.name){
            PC.footStep = ground.footStepSounds;
            currentground = ground.name;
        }
    }  
}

[System.Serializable]
public class GroundType {
    public string name;
    public AudioClip[] footStepSounds;
}