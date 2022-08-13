using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private EnchantableEntity enchantableEntity;

    [Header("Weapon Values")]
    [SerializeField] private int damage;
    [SerializeField] private float knockback;
    [SerializeField] private float attackRate;
    [SerializeField] private float shotSpeed;
    [SerializeField] private float range; // number of tiles travel
    [SerializeField] private float accuracy; 

    [SerializeField] private float homingSpeed = 200f;
    [SerializeField] private float homingRange = 2;

    [SerializeField] private float accelerationRate = 0.5f;
    [SerializeField] private float acceleration = 0.1f;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform holder;

    [SerializeField] private bool isMelee = false;
    [SerializeField] private int offset = 0;
    [SerializeField] private float positionOffset = 0;
    [SerializeField] private float animationDuration = 0.01f;

    [SerializeField] private float attackMovepeed = 50f;

    [SerializeField] private List<Enchantment> enchantments;

    private float attackTimer = 0;
    private bool isFiring = false;

    private void Awake() {
        enchantableEntity = GetComponent<EnchantableEntity>();
    }

    private void Start() {
        if (enchantments != null) {
            foreach (var enchantment in enchantments) {
                enchantableEntity.addEnchantment(enchantment);
            }
            enchantments.Clear();
        }
    }

    public void startFiring() {
        isFiring = true;
    }

    private void FixedUpdate() {
        AimTowards(CursorControl.instance.transform.position);

        // Charge between attack
        if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
        }
        else if (isFiring) {

            // Do temp 0 offset
            AimTowards(CursorControl.instance.transform.position);

            // Add random deviation based on accuracy
            var euler = firePoint.rotation.eulerAngles;
            euler.z += Random.Range(-accuracy, accuracy);

            var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(euler)).GetComponent<Projectile>();
            projectile.InitializeCore(damage, knockback, range, shotSpeed, gameObject);

            projectile.InitializeHoming(homingSpeed, homingRange);

            projectile.InitializeAcceleration(accelerationRate, acceleration);

            if (isMelee) {
                StartCoroutine(Swing(animationDuration));
            }
            else {
                StartCoroutine(Recoil(animationDuration));
            }

            // Trigger event
            GameEvents.instance.TriggerOnFire(this, projectile);

            // Set timer
            attackTimer = attackRate;
        }
    }

    private IEnumerator Swing(float duration) {
        // Save values
        float elapsedTime = 0f;

        // Start swing
        while (elapsedTime < duration)
        {
            // Slerp position
            holder.localPosition = Vector3.Slerp(new Vector3(0, positionOffset, 0),
                                                new Vector3(0, -positionOffset, 0), 
                                                elapsedTime / duration);

            // Slerp rotation
            holder.localRotation = Quaternion.Slerp(Quaternion.Euler(new Vector3(0, 0, offset)), 
                                                Quaternion.Euler(new Vector3(0, 0, -offset)),
                                                elapsedTime / duration);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set rotation and position
        holder.localPosition = new Vector3(0, -positionOffset, 0);
        holder.localRotation = Quaternion.Euler(new Vector3(0, 0, -offset));

        // Flip offset
        offset = -offset;
        positionOffset = -positionOffset;
    }

    private IEnumerator Recoil(float duration) {
        // Save values
        float elapsedTime = 0f;

        // Set rotation and position
        holder.localPosition = new Vector3(-positionOffset, 0, 0);

        // Slow down
        transform.parent.GetComponent<Movement>().SetMovespeed(attackMovepeed);

        // Start recoil
        while (elapsedTime < duration)
        {
            // Slerp position
            holder.localPosition = Vector3.Slerp(new Vector3(-positionOffset, 0, 0),
                                                new Vector3(0, 0, 0), 
                                                elapsedTime / duration);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Return speed
        transform.parent.GetComponent<Movement>().SetMovespeed(200f);

        // Set rotation and position
        holder.localPosition = new Vector3(0, 0, 0);
    }

    public void stopFiring() {
        isFiring = false;
    }

    public void AimTowards(Vector3 location) {
        Vector3 direction = (location - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.eulerAngles = new Vector3(0, 0, angle);

        // FLIP SPRITE IF ON THE OTHER SIDE
        if (angle < -90 || angle > 90) {
            // If facing 
            if (transform.parent.eulerAngles.y == 0) {
                transform.localRotation = Quaternion.Euler(180, 0, -angle);
            }
            else if (transform.parent.eulerAngles.y == 180) {
                transform.localRotation = Quaternion.Euler(180, 180, -angle);
            }
        }
    }

}
