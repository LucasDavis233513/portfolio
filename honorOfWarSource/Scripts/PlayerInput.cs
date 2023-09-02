using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

namespace UnityStandardAssets.Characters.PlayerController {
    [RequireComponent(typeof (AudioSource))]

    public class PlayerInput : MonoBehaviour{
        [Header("Camera Object")]
        [SerializeField] GameObject cameraObject;
        [SerializeField] MouseLook ML;

        [Header("Player Health")]
        [SerializeField] float MaxHealth = 10;
        public float HP;
        public healthBar hpBar; 

        [Header("Movement Variables")]
        [SerializeField] float speed = 15;
        [SerializeField] float jumpVelocity = 10f;
        public bool gameOver = false;
        private bool victory = false;
        private Vector2 movementVector;
        private bool isGrounded;
        private float movementX, movementZ;
        private Rigidbody rb;

        [Header("Draw Box Gizmo")]
        [SerializeField] Vector3 boxSize;
        [SerializeField] float maxDistance;
        [SerializeField] LayerMask layerMask;

        [Header("Audio variables")]
        [SerializeField] AudioClip[] Clips;
        public AudioClip[] footStep;
        [SerializeField] AudioSource pickUpSource;
        private AudioSource footStepSounds;
        private bool played = false;

        [Header("Score UI")]
        [SerializeField] TextMeshProUGUI countText;
        [SerializeField] GameObject winTextObject;
        private int count;

        [HideInInspector] public bool canSprint;
        [HideInInspector] public bool weAreSprinting = false;
        private bool fireTrigger;

        void Awake() {
            SetCountText();
        }

        void Start() {
            winTextObject.SetActive(false);

            rb = this.GetComponent<Rigidbody>();

            footStepSounds = this.gameObject.GetComponent<AudioSource>();
            footStepSounds.volume = volControler.targetVolumeControl;
            pickUpSource.volume = volControler.targetVolumeControl;

            HP = MaxHealth;
            hpBar.SetMaxHealth(MaxHealth);

            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update() {
            OnMove();

            if(Input.GetKeyDown(KeyCode.Space) && GroundCheck() && (Pause.isGamePaused == false))
                rb.AddForce(transform.up * jumpVelocity, ForceMode.Impulse);

            if(gameOver == true) {
                if(played != true && HP > 0) {
                    footStepSounds.Stop();
                    victorySound();

                    victory = true;
                    played = true;
                } else if(played != true && HP <= 0) {
                    footStepSounds.Stop();
                    deathSound();

                    played = true;
                }

                Restart();
            } else {
                Handle_FootSteps();
            }
        }
        private void Restart() {
            if(Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if(Input.GetKeyDown(KeyCode.Q))
                Application.Quit();
        }

        private void OnTriggerEnter(Collider other) {
            if(other.CompareTag("Bullet")) {
                Debug.Log("Player Hit");
                
                if(!(MouseLook.isShaking))
                    StartCoroutine(ML.Shaking());

                Destroy(other.gameObject);
                pickUpSource.PlayOneShot(Clips[4]);

                HP -= 2; 
                hpBar.SetHealth(HP);         
            } else if(other.CompareTag("Pickup")) {
                Pickup.item = other;                //Set the item collider in Pickup script
                StartCoroutine(pickupDelay(other)); //start courtine to disable pickup object
                pickUpSource.PlayOneShot(Clips[2]);

                if(Pickup.pickedUp == false) {
                    count++;

                    HP += 1;
                    hpBar.SetHealth(HP);
                }

                SetCountText();
            } 
        }

        private void OnTriggerStay(Collider other) {
            if(other.CompareTag("Fire")) {
                if(!fireTrigger)
                    StartCoroutine(fireDelay());
            }
        }

        private void OnMove() {
            movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            movementX = movementVector.x;
            movementZ = movementVector.y;
        }

        private void FixedUpdate() {
            Vector3 movement = new Vector3(movementX, rb.velocity.y, movementZ);
            Vector3 direction = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
            Vector3 velocity;
            direction.Normalize();

            //Sprinting functionality
            if(Input.GetKey(KeyCode.LeftShift) && canSprint) {
                weAreSprinting = true;
                velocity = direction * (speed * 2);
            } else {
                weAreSprinting = false;
                velocity = direction * speed;
            }

            if(victory == true)
                transform.rotation = Quaternion.Euler(0f, cameraObject.GetComponent<MouseLook>().currentY, 0f);
            
            
            if((gameOver != true) && (HP > 0) && (Pause.isGamePaused == false)){
                transform.rotation = Quaternion.Euler(0f, cameraObject.GetComponent<MouseLook>().currentY, 0f); 
                rb.AddForce(movement + velocity);
            }                               
        }

        private void Handle_FootSteps() {
            if(GroundCheck() && (movementVector != Vector2.zero) && (Pause.isGamePaused == false)) {
                if(!footStepSounds.isPlaying){
                    if(weAreSprinting)
                        footStepSounds.PlayOneShot(Clips[3]);
                    else
                        footStepSounds.PlayOneShot(footStep[0]);
                }
            } else {
                footStepSounds.Stop();
            }
        }

        private void victorySound() {
            if(!footStepSounds.isPlaying)
                footStepSounds.PlayOneShot(Clips[0]);
        }

        private void deathSound() {
            if(!footStepSounds.isPlaying)
                footStepSounds.PlayOneShot(Clips[1]);
        }

        private void OnDrawGizmos() {
            Gizmos.color=Color.red;
            Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
        }

        private bool GroundCheck() {
            if (Physics.BoxCast(transform.position, boxSize, -transform.up, transform.rotation, maxDistance, layerMask))
                return true;
            else
                return false;
        }

        void SetCountText() {
            countText.text = "Plans Recovered: " + count.ToString() + "/8";

            if (count >= 8){
                gameOver = true;
                winTextObject.SetActive(true);
            }
        }

        //This is the deplay for the pickup disable
        //This will allow any animations play before disabling the object
        IEnumerator pickupDelay(Collider pickup) {
            yield return new WaitForSeconds(2);
            Pickup.pickedUp = false;                //Reset Pickup flags
            Pickup.triggered = false;               //-----||-------
            pickup.gameObject.SetActive(false);
        }

        IEnumerator fireDelay() {
            fireTrigger = true;
            pickUpSource.PlayOneShot(Clips[5]);
            yield return new WaitForSeconds(1);
            HP -= 1;
            hpBar.SetHealth(HP);
            fireTrigger = false;
        }        
    }
}