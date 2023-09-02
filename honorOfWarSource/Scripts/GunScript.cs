using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityStandardAssets.Characters.PlayerController;
using DG.Tweening;

public class GunScript : MonoBehaviour {
    //Same as private, but we can only access it from this script
    //Will still show up in the unity toolbar.
    [Header("Bullet Controls")]
    [SerializeField] float bulletVelocity = 30f;
    [SerializeField] double fireRate = 0.1;
    [SerializeField] float timetoDestroy = 3;

    [Header("Prefabs")]
    [SerializeField] GameObject Shell;
    [SerializeField] GameObject Bullet;

    [Header("Spawn Positions")]
    public Transform shellSpawnPos; 
    public Transform bulletSpawnPos;
    RaycastHit variable; 
    private Rigidbody rb;
    private double nextFire;

    [Header("Gun Fire sound")]
    [SerializeField] AudioClip fireSound;
    [SerializeField] AudioClip gunReloadSound;
    private AudioSource fireSource;

    [Header("Recoil Object")]
    private Animator gunAim;

    [Header("Player")]
    [SerializeField] PlayerInput PI;
    
    //Awake function is ran before the start function.
    void Awake() {
        //This and the comment above and bellow, labeled 1 & 2, is how you reference another script
        rb = Bullet.GetComponent<Rigidbody>();
        fireSource = this.gameObject.GetComponent<AudioSource>();
        fireSource.volume = volControler.targetVolumeControl;

        gunAim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        Shoot();
    }

    void Shoot() {
        if (Input.GetButtonDown ("Fire1") && (Time.time > nextFire) && (Pause.isGamePaused == false) && (PI.gameOver != true)){
            nextFire = Time.time + fireRate;

            Fire();
        }
    }

    void Fire() {
        GameObject shellCopy = Instantiate<GameObject> (Shell, shellSpawnPos.position, Quaternion.identity) as GameObject; 
        bool status = Physics.Raycast (bulletSpawnPos.position, bulletSpawnPos.forward, out variable, 100);

        if (status)
            Debug.Log(variable.collider.gameObject.name);

        //This is referancing a function within the ShellScript Function
        addForce(bulletSpawnPos);  //2
        Destroy(shellCopy, timetoDestroy);

        StartCoroutine(FireGun());
    }

    private void addForce(Transform bulletSpawnPos) {
        GameObject bulletCopy = Instantiate(Bullet, bulletSpawnPos.position, bulletSpawnPos.rotation);

        Rigidbody rb = bulletCopy.GetComponent<Rigidbody>();

        rb.AddForce(bulletSpawnPos.forward * bulletVelocity, ForceMode.Impulse);
        
        Destroy(bulletCopy, timetoDestroy);
    }

    IEnumerator FireGun() {
        gunAim.Play("GunRecoil");
        fireSource.PlayOneShot(fireSound);
        
        yield return new WaitForSeconds(1);
        
        gunAim.Play("GunPause");
        fireSource.PlayOneShot(gunReloadSound);
    }
}
