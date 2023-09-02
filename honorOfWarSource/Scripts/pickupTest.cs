using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickupTest : MonoBehaviour
{
    public static bool EnteredTrigger;

    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("Player"))
            EnteredTrigger = true;
    }
}
