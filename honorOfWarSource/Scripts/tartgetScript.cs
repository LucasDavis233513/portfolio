using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tartgetScript : MonoBehaviour{
    [Header("Hit Sound")]
    [SerializeField] AudioClip hitSound;

    [Header("Hit Force")]
    [SerializeField] float targetForce = 100f;
    private Rigidbody rbTarget;

    void Start() {
        rbTarget = this.GetComponent<Rigidbody>();
    }

    //This is an ontriggerenter event, once a collider has triggered the traget this script will run
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Bullet")){
            Debug.Log("Target Hit");
            //This will destroy the collision item Bullet.
            Destroy(other.gameObject);
            AudioSource.PlayClipAtPoint(hitSound, transform.position);

            StartCoroutine(triggerCoroutine());            
        }
    }

    IEnumerator triggerCoroutine() {
        rbTarget.AddForce(new Vector3 (-targetForce, 0f , 0f), ForceMode.Impulse);
        rbTarget.useGravity = true;
        yield return new WaitForSeconds(5);
        this.gameObject.SetActive(false);
    }
}
