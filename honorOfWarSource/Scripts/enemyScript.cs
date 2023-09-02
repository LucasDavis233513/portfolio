using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Sound.MusicPlayer;
using UnityStandardAssets.Characters.PlayerController;
using TMPro;

public class enemyScript : MonoBehaviour{
    [Header("Enemy Check Variables")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform player;
    [SerializeField] Transform projectileSpawnPos;
    [SerializeField] LayerMask whatIsGround, whatIsPlayer;
    [SerializeField] float timetoDestroy = 1;
    [SerializeField] float MaxHPEnemy = 5;
    public float healthEnemy;
    public enemyHPBar ehpBar;

    [Header("enemyMusic")]
    [SerializeField] MusicPlayer MP;
    [SerializeField] AudioClip fireSound;
    [SerializeField] AudioClip reloadSound;
    private AudioSource audioSource;

    [Header("Patroling Variables")]
    public Vector3 walkPoint;
    public float walkPointRange;
    private bool walkPointSet;

    [Header("Attacking")]
    [SerializeField] float timeBetweenAttacks;
    [SerializeField] float timeBetweenMovements;
    [SerializeField] GameObject projectile;
    private bool alreadyAttacked;

    //States
    [SerializeField] float sightRange, attackRange;
    private bool playerInSightRange, playerInAttackRange;
    private bool canWalk;

    [Header("Score UI")]
    [SerializeField] PlayerInput PI;
    [SerializeField] GameObject WastedObject;

    private bool switchAudio;
    private bool slowedTime;

    void Awake() {
        WastedObject.SetActive(false);

        healthEnemy = MaxHPEnemy;
        ehpBar.SetMaxHealth(MaxHPEnemy);

        audioSource = this.GetComponent<AudioSource>();
        audioSource.volume = volControler.targetVolumeControl;
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        SetCountText();
    }

    void Update() {
        //check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if(healthEnemy > 0) {
            if(!playerInSightRange && !playerInAttackRange) Patroling();
            if(playerInSightRange && !playerInAttackRange) ChasePlayer();
            if(playerInAttackRange && playerInSightRange) AttackPlayer();
        }

        SetCountText();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Bullet")){
            Debug.Log("Target Hit");
            //This will destroy the collision item Bullet.
            Destroy(other.gameObject);
            //AudioSource.PlayClipAtPoint(hitSound, transform.position);

            Invoke(nameof(damageRoutine), 0f);

            ChasePlayer();          
        }
    }
    
    private void Patroling() {
        if(!canWalk){
            if(!walkPointSet) SearchWalkPoint();

            if(walkPointSet)
                agent.SetDestination(walkPoint);

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //Walkpoint reached
            if(distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
            canWalk = true;

            Invoke(nameof(resetWalk), timeBetweenMovements);
        }
    }

    private void SearchWalkPoint() {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer() {
        agent.SetDestination(player.position);
        MP.enemy = true;
        if(!switchAudio){
            MP.playOnce = false;
            switchAudio = true;
        }
    }

    private void AttackPlayer() {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if(!alreadyAttacked) {
            //Attack Code here
            GameObject projectileClone = Instantiate(projectile, projectileSpawnPos.position, projectileSpawnPos.rotation);
            Rigidbody rb = projectileClone.GetComponent<Rigidbody>();

            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);

            StartCoroutine(gunSoundCoroutine());

            Destroy(projectileClone, timetoDestroy);
            //

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);

        }
    }

    private void ResetAttack() {
        alreadyAttacked = false;
    }

    private void resetWalk() {
        canWalk = false;
    }

    private void damageRoutine() {
        healthEnemy--;
        ehpBar.SetHealth(healthEnemy); 

        if(healthEnemy <= 0) StartCoroutine(DestroyEnemy());
    }

    private void SetCountText() {
        if (PI.HP <= 0){
            MP.enemy = true;
            PI.gameOver = true;

            if(!slowedTime){
                StartCoroutine(slowDownTime());
                slowedTime = true;
            }
            
            WastedObject.SetActive(true);
        }
    }

    IEnumerator slowDownTime() {
        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(1);
        Time.timeScale = 1;
    }

    IEnumerator gunSoundCoroutine() {
        audioSource.PlayOneShot(fireSound);
        yield return new WaitForSeconds(1);
        audioSource.PlayOneShot(reloadSound);
    }

    IEnumerator DestroyEnemy() {
        MP.enemy = false;
        MP.playOnce = false;
        yield return new WaitForSeconds(2);
        this.gameObject.SetActive(false);
    }
}
