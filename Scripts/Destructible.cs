using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Destructible : MonoBehaviour
{
    [SerializeField] protected float startHealth = 5;
    [SerializeField] protected float immunityTime = 0.3f;
    protected float health;
    protected float immunityTimer;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        health = startHealth;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        immunityTimer += Time.deltaTime;
    }

    public virtual void ProcessHit(float damage)
    {
        if (immunityTimer >= immunityTime)
        {

            immunityTimer = 0f;
            health -= damage;
            print("hit");
            if (health <= 0)
            {
                ProcessDeath();
            }
        }
    }

    public virtual void ProcessDeath()
    {
        Destroy(gameObject);
    }
}
