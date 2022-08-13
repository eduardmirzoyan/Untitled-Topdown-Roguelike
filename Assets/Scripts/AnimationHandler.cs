using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string currentState;

    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void changeAnimationState(string newState)
    {
        // Guard to prevent replaying same state
        if (currentState == newState)
            return;
            
        // Play animation
        animator.Play(newState);

        // Reassign current state
        currentState = newState;
    }

    public float getAnimationDuration() {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }
}
