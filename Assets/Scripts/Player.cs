using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    idle,
    walking,
    attacking,
    interacting,
    inMenu,
    knockedback,
    dead
}

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerState state;

    [Header("Components")]
    [SerializeField] private Movement mv;
    [SerializeField] private AnimationHandler animationHandler;
    [SerializeField] private WeaponHandler weaponHandler;

    private PlayerControls playerControls;
    Vector2 direction;

    private void Awake() {
        mv = GetComponent<Movement>();
        animationHandler = GetComponent<AnimationHandler>();
        weaponHandler = GetComponent<WeaponHandler>();
    }

    private void Start() {
        playerControls = GameManager.instance.GetInputActions();

        playerControls.Player.Attack.performed += fire;
        playerControls.Player.ReleaseAttack.performed += release;
        
    }

    private void Update() {
        direction = playerControls.Player.Move.ReadValue<Vector2>();
    }

    private void fire(InputAction.CallbackContext context) {
        weaponHandler.usePrimaryWeapon();
    }
    
    private void release(InputAction.CallbackContext context) {
        weaponHandler.releasePrimaryWeapon();
    }

    private void FixedUpdate() {
        mv.Walk(direction);

        if (direction != Vector2.zero) {
            animationHandler.changeAnimationState("Run");
        }
        else {
            animationHandler.changeAnimationState("Idle");
        }
    }
}
