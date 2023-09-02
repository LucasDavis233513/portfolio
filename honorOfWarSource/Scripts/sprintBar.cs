using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.PlayerController;

public class sprintBar : MonoBehaviour
{
    [Header("Stamina Main Parameters")]
    public float playerStamina = 10;
    [SerializeField] private float maxStamina = 10;
    [SerializeField] private bool hasRegenerated = true;
    
    [Header("Stamina Regen Parameters")]
    [Range(0,50)] [SerializeField] private float staminaDrain = 2;
    [Range(0,50)] [SerializeField] private float staminaRegen = 1;

    [Header("Stamina UI Elements")]
    [SerializeField] Slider slider;

    [Header("Player Controller Script")]
    [SerializeField] PlayerInput PI;

    void Start() {
        PI.canSprint = true;

        slider.maxValue = maxStamina;
        slider.value = playerStamina;
    }

    void Update() {
        if(PI.weAreSprinting == false) {
            if(playerStamina <= maxStamina - 0.01){
                playerStamina += staminaRegen * Time.deltaTime;
                slider.value = playerStamina;

                if(playerStamina >= maxStamina - 0.01){
                    Debug.Log("Has Regenerated, flag switch");
                    PI.canSprint = true;
                    hasRegenerated = true;
                }
            }
        } else {
            if(hasRegenerated){
                playerStamina -= staminaDrain * Time.deltaTime;
                slider.value = playerStamina;

                if(playerStamina <= 0){
                    hasRegenerated = false;
                    PI.canSprint = false;
                }
            }
        }
    }
}
