using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.PlayerController;
using DG.Tweening;

public class MouseLook : MonoBehaviour {
    [Header("Mouse Sensitivity")]
    public float lookSensitivity = 8f; 
    public float lookSmoothDamp = 0.5f;

    [Header("Script Information")]
    [SerializeField] PlayerInput PI;
    [SerializeField] GunScript GS;

    [Header("Shake Duration")]
    [SerializeField] float duration;
    [SerializeField] AnimationCurve curve;

    [Header("Hipfire Recoil")]
    [SerializeField] float maxRecoil_x; //-20
    [SerializeField] float recoilSpeed; //10

    //Camera Rotation Values
    [HideInInspector]                   //This is required to hide a variable from the unity engine so it can't be monipulated
                                        //but since this variable is used in the PlayerInput class it needs to be public.
    public float currentY, currentX;
    [HideInInspector]
    public float yRot, xRot;
    [HideInInspector]
    public float yRotationV, xRotationV;

    public static bool isShaking;
    public static Vector3 StartPosition;

    public void LateUpdate() {
        yRot += Input.GetAxis("Mouse X") * lookSensitivity; 
        xRot += Input.GetAxis("Mouse Y") * lookSensitivity;

        currentX = Mathf.SmoothDamp(currentX, xRot, ref xRotationV, lookSmoothDamp);
        currentY = Mathf.SmoothDamp(currentY, yRot, ref yRotationV, lookSmoothDamp);

        xRot = Mathf.Clamp(xRot, -80, 80);

        if((Pause.isGamePaused != true) && (PI.gameOver != true))
           transform.rotation = Quaternion.Euler(-currentX, currentY, 0);

        // Recoil();
    }

    // public void Recoil() {
    //     if(GS.Fired == true) {
    //         if(GS.Recoil > 0) {
    //             var maxRecoil = Quaternion.Euler (maxRecoil_x * -currentX, 0, 0);
    //             // Dampen towards the target rotation
    //             transform.localRotation = Quaternion.Slerp(transform.localRotation, maxRecoil, Time.deltaTime * recoilSpeed);
    //             GS.Recoil -= Time.deltaTime;
    //         } else {
    //             GS.Recoil = 0;
    //             var minRecoil = Quaternion.Euler (-currentX, currentY, 0);
    //             // Dampen towards the target rotation
    //             transform.localRotation = Quaternion.Slerp(transform.localRotation, minRecoil,Time.deltaTime * recoilSpeed / 2);
    //             GS.Fired = false;
    //         }
    //     }
    // }

    public IEnumerator Shaking() {
        isShaking = true;
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration){
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.localPosition = startPosition + (Random.insideUnitSphere * strength);
            yield return null;
        } 

        transform.localPosition = startPosition;
        isShaking = false;
    }
}