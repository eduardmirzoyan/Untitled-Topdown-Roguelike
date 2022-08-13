using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float movespeed = 200f;
    

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Walk(Vector2 direction) {
        rb.velocity = direction * movespeed * Time.deltaTime;
    }

    public void SetMovespeed(float speed) {
        movespeed = speed;
    }
}
