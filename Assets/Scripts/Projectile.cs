using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows components to fly through the air
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private GameObject source;
    [SerializeField] private GameObject hitEffect;

    [Header("Core Properties")]
    [SerializeField] private int damage = 0;
    [SerializeField] private float knockback = 0;
    [SerializeField] private float range = 1;
    [SerializeField] private float speed = 0f;

    [Header("Secondary Properties")]
    [SerializeField] private bool isSpectral = false;
    [SerializeField] private int numberOfBounces = 0;
    [SerializeField] private int numberOfPierces = 0;

    [Header("Homing")]
    [SerializeField] private Transform target = null;
    [SerializeField] private float homingSpeed = 0;
    [SerializeField] private float homingRange = 0;

    [Header("Acceleration")]
    [SerializeField] private float accelerationRate = 0;
    [SerializeField] private float acceleration = 0;

    [SerializeField] private bool isCopy = false;
    private float accelerationTimer;
    private float lifetime;


    // Start is called before the first frame update
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        // Track range
        if (lifetime > 0) {
            lifetime -= Time.deltaTime;
        }
        else {
            Fizzle();
        }

        // Check if to home
        if (homingRange > 0) {
            
            // If target found, then home in
            if (target != null) {
                // If you get too far from target, then drop it
                if (Vector3.Distance(target.position, transform.position) > homingRange) {
                    body.angularVelocity = 0;
                    body.velocity = transform.right * speed;
                    target = null;
                    return;
                }

                Vector2 direction = (Vector2) target.position - body.position;
                direction.Normalize();
                float rotateAmount = Vector3.Cross(direction, transform.right).z;
                body.angularVelocity = -rotateAmount * homingSpeed;
                body.velocity = transform.right * speed;
            }
            else { // Look for target
                var hit = Physics2D.OverlapCircle(transform.position, homingRange, 1 << LayerMask.NameToLayer("Enemies"));
                if (hit) {
                    target = hit.transform;
                }
            }
        }

        if (accelerationRate > 0) {
            if (accelerationTimer > 0) {
                accelerationTimer -= Time.deltaTime;
            }
            else
            {
                body.velocity *= (1 + acceleration);
                accelerationTimer = accelerationRate;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Enemy") {
            // Attempt to pierce if possible
            if (numberOfPierces > 0) {
                // Don't destroy
                numberOfPierces--;
            }
            else {
                Fizzle();
            }
        }

        if (other.tag == "Wall" && !isSpectral) {
            // Attempt to bounce if possible
            if (numberOfBounces > 0) {
                Bounce();
                numberOfBounces--;
            }
            else {
                Fizzle();
            }
        }
    }

    public void InitializeCore(int damage, float knockback, float range, float speed, GameObject source) {
        this.damage = damage;
        this.knockback = knockback;
        this.range = range;
        this.speed = speed;
        this.source = source;

        // Calculate lifetime
        lifetime = range / speed;


        body.velocity = transform.right * this.speed;
    }

    public void InitializeSecondary(float sizeMulti, int pierces, int bounces) {
        // Change size
        setSize(sizeMulti);

        // Set pierces
        numberOfPierces = pierces;

        // Set bounces
        numberOfBounces = bounces;
    }

    public void setSize(float sizeMulti) {
        transform.localScale = Vector3.one * sizeMulti;
    }

    public void SetRange(float range) {
        this.range = range;
        lifetime = range / speed;
    }

    public void SetDamage(int damage) {
        this.damage = damage;
    }

    public void InitializeHoming(float homingSpeed, float homingRange) {
        this.homingSpeed = homingSpeed;
        this.homingRange = homingRange;
    }

    public void InitializeAcceleration(float accelerationRate, float acceleration) {
        this.accelerationRate = accelerationRate;
        this.acceleration = acceleration;
    }

    private void Fizzle() {
        if (hitEffect != null) {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Trigger event
        GameEvents.instance.TriggerOnHit(this);

        Destroy(gameObject);
    }

    public void Copy(Projectile projectile) {
        InitializeCore(projectile.damage, projectile.knockback, projectile.range, projectile.speed, projectile.source);
        InitializeSecondary(projectile.transform.localScale.x, projectile.numberOfPierces, projectile.numberOfBounces);
        InitializeHoming(projectile.homingSpeed, projectile.homingRange);
        InitializeAcceleration(projectile.accelerationRate, projectile.acceleration);

        isSpectral = projectile.isSpectral;

        isCopy = true;
    }

    private void Bounce() {
        var hit = Physics2D.Raycast(transform.position, body.velocity.normalized, 1f, 1 << LayerMask.NameToLayer("Wall"));
        if (hit)
        {
            var newDirection = Vector3.Reflect(body.velocity.normalized, hit.normal);
            body.velocity = body.velocity.magnitude * newDirection;
            turn();
        }
    }

    public void FlagAsCopy() {
        isCopy = true;
    }

    public bool IsCopy() {
        return isCopy;
    }

    public void freezePosition() {
        body.isKinematic = true;
        body.velocity = Vector2.zero;
    }

    // Reverse the velocity of the projectile
    public void reverseVelocity()
    {
        body.velocity = -body.velocity.normalized * speed;
        transform.Rotate(0f, 180f, 0f);
    }

    private void turn() {
        float angle = Mathf.Atan2(body.velocity.y, body.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public GameObject getSource() {
        return source;
    }

    private void OnDrawGizmos() {
        if (homingRange > 0) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, homingRange);
        }
    }

}
