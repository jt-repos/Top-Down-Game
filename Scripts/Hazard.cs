using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : Destructible
{
    [SerializeField] float timeToResetCollision = 1f;
    [SerializeField] float damage = 1;
    [SerializeField] bool isBlocking;
    bool isEnabled = true;


    public void SetCollisions(bool boolVar)
    {
        GetComponent<BoxCollider>().enabled = boolVar;
        if (boolVar == false)
        {
            StartCoroutine(ResetCollision());
        }
    }

    private IEnumerator ResetCollision()
    {
        if(isBlocking)
        {
            Destroy(gameObject);
        }
        yield return new WaitForSeconds(timeToResetCollision);
        SetCollisions(true);
    }

    public float GetDamage()
    {
        return damage;
    }

    public void SetIsEnabled(bool boolVar)
    {
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        isEnabled = boolVar;
    }

    public bool GetIsEnabled()
    {
        return isEnabled;
    }
}
