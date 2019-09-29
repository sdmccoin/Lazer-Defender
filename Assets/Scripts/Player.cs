using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] float moveSpeed;
    [SerializeField] float viewPortPadding = 1f;
    [SerializeField] int health = 100;
    [SerializeField] [Range(0, 1)] float deathSoundVolume = 0.75f;
    [SerializeField] [Range(0, 1)] float shootSoundVolume = 0.25f;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip shootSound;
    [SerializeField] GameObject deathVFX;
    [SerializeField] float durationOfExplosion = 1f;

    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = .05f;
    [SerializeField] float projectileFireingPeriod = 0.1f;
    
   

    Coroutine firingCoroutine;

    float xMin;
    float xMax;

    float yMin;
    float yMax;

    // Start is called before the first frame update
    void Start()
    {
        SetupMoveBounderies();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire();
    }

    private void Fire()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            firingCoroutine = StartCoroutine(FireContinuously());  
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);
        }
    }

    private IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject laser =
             Instantiate(laserPrefab,
             transform.position,
             Quaternion.identity) as GameObject;

            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, shootSoundVolume);
            yield return new WaitForSeconds(projectileFireingPeriod);
        }       
    }

    private void SetupMoveBounderies()
    {
        Camera mainCamera = Camera.main;
        xMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + viewPortPadding;
        xMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - viewPortPadding;

        yMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + viewPortPadding;
        yMax = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - viewPortPadding;
    }

    private void Move()
    {
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);

        transform.position = new Vector2(newXPos, newYPos);        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) { return; }
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();
        damageDealer.Hit();
        if (health <= 0)
        {
            Die();
        }        
    }

    private void Die()
    {
        FindObjectOfType<Level>().LoadGameOver();
        Destroy(gameObject);
        GameObject explosion = Instantiate(deathVFX, transform.position, transform.rotation);
        Destroy(explosion, durationOfExplosion);
        AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position, deathSoundVolume);
    }

    public int GetHealth()
    {
        return health;
    }
}
