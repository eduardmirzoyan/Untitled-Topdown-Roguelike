using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteHandler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isFacingRight = true;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate() {
        var delta = CursorControl.instance.transform.position - transform.position;

        // if (direction.x > 0.1f && !isFacingRight || direction.x < -0.1f && isFacingRight)
        // {
        //     isFacingRight = !isFacingRight;
        //     transform.Rotate(0f, 180f, 0f);
        // }

        if (delta.x >= 0 && !isFacingRight) { // mouse is on right side of player
            //transform.localScale = new Vector3(1,1,1); // or activate look right some other way
            transform.Rotate(0f, 180f, 0f);
            //spriteRenderer.flipX = false;
            isFacingRight = true;
        } else if (delta.x < 0 && isFacingRight) { // mouse is on left side
            //transform.localScale = new Vector3(-1,1,1); // activate looking left
            transform.Rotate(0f, 180f, 0f);
            //spriteRenderer.flipX = true;
            isFacingRight = false;
        }
    }

    public void Flip() {
        
    }
     
    public int GetFacingDirection()
    {
        return isFacingRight ? 1 : -1;
    }

    public void SetFacingDirection(float direction)
    {
        if (direction < 0 && isFacingRight)
        {
            isFacingRight = false;
            transform.Rotate(0f, 180f, 0f);
        }
        else if (direction > 0 && !isFacingRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
    }
}
