using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour {
    [Header("Popus Dialog box")]
    [SerializeField] GameObject[] popup;
    [SerializeField] GameObject parent;

    [Header("Cycle speed")]
    [SerializeField] int timeBetweenPopups;

    private int count = 0;
    private bool end;

    void Start(){
        StartCoroutine(iterate());
    }
    
    IEnumerator iterate() {
        for(int i = 0; i < popup.Length - 1; i++) {
            yield return new WaitForSeconds(timeBetweenPopups);

            if(!end){
                popup[count].SetActive(false);
                popup[count + 1].SetActive(true);
                count++;
            }

            if(count == popup.Length - 1)
                end = true;
        }

        if(end){
            yield return new WaitForSeconds(timeBetweenPopups);
            parent.SetActive(false);
        }
    }
}
