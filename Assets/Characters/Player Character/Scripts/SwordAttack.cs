using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    //public enum AttackDirection
    //{
    //    left, right
    //}

    //public AttackDirection attackDirection;

    Vector2 attackOffset;
    [SerializeField]
    Collider2D swordCollider;
    [SerializeField]
    float damage = 1f;

    // Start is called before the first frame update
    void Start()
    {
        attackOffset = transform.localPosition;
    }

    //public void Attack()
    //{
    //    switch(attackDirection)
    //    {
    //        case AttackDirection.left:
    //            AttackLeft(); 
    //            break;
    //        case AttackDirection.right:
    //            AttackRight(); 
    //            break;
    //    }
    //}

    public void AttackLeft()
    {
        print("Attack left");
        swordCollider.enabled = true;
        swordCollider.attachedRigidbody.WakeUp();
        transform.localPosition = attackOffset;
    }

    public void AttackRight()
    {
        print("Attack right");
        swordCollider.enabled = true;
        swordCollider.attachedRigidbody.WakeUp();
        transform.localPosition = new Vector2(attackOffset.x * -1, attackOffset.y);
    }

    public void StopAttack()
    {
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy") 
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if(enemy != null)
            {
                enemy.Health -= damage;
                //enemy.animator.SetTrigger("TakesDamage");
            }
        }
    }
}
