using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Vector coords for player movement
    Vector2 movementInput;
    // Instantiate player rigidbody
    Rigidbody2D rb;

    // Movement speed value
    [SerializeField]
    float moveSpeed = 1f;
    bool canMove = true;

    // Collision variables
    // How far away from the player until collision is detected
    [SerializeField]
    float collisionOffset = 0.05f;
    // Determines layers to collide with
    public ContactFilter2D contactFilter;
    // List of collisions to store after the Cast is complete
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();

    // Setting up for animation
    Animator animator;
    SpriteRenderer spriteRenderer;

    // Determine which direction a sword attack should swing
    public SwordAttack swordAttack;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();   
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            if (movementInput != Vector2.zero)
            {
                // Try regular movement (both axis)
                bool result = TryMove(movementInput);

                if (!result)
                {
                    // Try move only on x-axis
                    result = TryMove(new Vector2(movementInput.x, 0));

                    if (!result)
                    {
                        //Try move only on y-axis
                        result = TryMove(new Vector2(0, movementInput.y));
                    }
                }

                animator.SetBool("isMoving", result);
            }
            else animator.SetBool("isMoving", false);

            // Set direction of sprite to movement direction
            if (movementInput.x < 0)
            {
                spriteRenderer.flipX = false;
                //swordAttack.attackDirection = SwordAttack.AttackDirection.left;
            }
            else if (movementInput.x > 0)
            {
                spriteRenderer.flipX = true;
                //swordAttack.attackDirection = SwordAttack.AttackDirection.right;
            }

            // Minimum code to get the sprite to move with player input
            //rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // Used to move around objects on a single axis if one axis experiances a collision, but the other does not.
    // Original code stopped movement in all directions if a collision occured.
    private bool TryMove(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            // This code is required to use raycasts to determine if the player (rb) will collide with any collider objects
            // and allow the collision to happen
            int count = rb.Cast(
                    direction,
                    contactFilter,
                    castCollisions,
                    moveSpeed * Time.fixedDeltaTime + collisionOffset);

            if (count == 0)
            {
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
                return true;
            }
            else return false;
        }

        else return false;   
    }

    // Get Vector coords (x & y) from player input component
    void OnMove(InputValue input)
    {
        movementInput = input.Get<Vector2>();
    }

    void OnFire()
    {
        //print("fire pressed");
        animator.SetTrigger("swordAttack");
    }

    // Perform the sword attack in the correct direction
    public void SwordAttack()
    {
        lockMovement();

        if(spriteRenderer.flipX == true)
        {
            swordAttack.AttackRight();
        }
        else { swordAttack.AttackLeft(); }
    }

    public void lockMovement()
    {
        canMove = false;
    }

    public void unlockMovement()
    {
        canMove = true;
        swordAttack.StopAttack();
    }
}
