using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {
    [Header("Upward force")]
    [SerializeField] float force;
    // [SerializeField] float rotationTime;

    [Header("Document Prefabs")]
    [SerializeField] GameObject[] prefab;

    [Header("Particles")]
    [SerializeField] GameObject[] particle;

    [Header("Player Desination")]
    [SerializeField] Transform player;

    public static bool pickedUp;        //Picked up flag, reset in playerinput.cs
    public static bool triggered;       //Trigger flag for the coroutine, reset in plauerinput.cs
    public static Collider item;        //This is a collider item that is being set in playerinput.cs
    private int count; 

    void Update() {
        for(int i=0; i<prefab.Length; i++){
            prefab[i].transform.Rotate (new Vector3(0, 0, 30) * Time.deltaTime);
        }

        if(pickupTest.EnteredTrigger){
            for(int j=0; j<prefab.Length; j++){
                if (prefab[j].gameObject.name == item.gameObject.name){
                    Debug.Log("item detected");
                    count = j;
                    break;
                }
            }
            pickedUp = true;
            pickupTest.EnteredTrigger = false;
        }

        if(pickedUp){
            prefab[count].transform.Rotate (new Vector3(0, 0, 360) * Time.deltaTime);
            if(!triggered)
                StartCoroutine(triggerCoroutine(prefab[count].GetComponent<Rigidbody>(), particle[count]));
        }
    }

    IEnumerator triggerCoroutine(Rigidbody rbTest, GameObject p) {
        triggered = true;
        rbTest.AddForce(new Vector3 (0f, force, 0f), ForceMode.Impulse);
        yield return new WaitForSeconds(1);
        rbTest.useGravity = true;
        yield return new WaitForSeconds(1);
        p.SetActive(true);
        yield return new WaitForSeconds(2);
        p.SetActive(false);
    }
}
