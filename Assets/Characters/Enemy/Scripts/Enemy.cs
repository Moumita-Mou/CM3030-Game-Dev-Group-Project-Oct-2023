using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;

    public float health = 1f;
    public float Health
    {
        set
        {
            health = value;

            if(health <= 0)
            {
                EnemyDies();
            }
        }

        get { return health; }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void EnemyDies()
    {
        animator.SetTrigger("EnemyDies");
        RemoveEnemy(0.35f);
    }

    public void RemoveEnemy(float delay)
    {
        Destroy(gameObject, delay);
    }


}
