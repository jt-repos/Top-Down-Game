using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObstacleCube : Hazard
{

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //Debug.Log(immunityTimer);
    }

    public override void ProcessDeath()
    {
        SetIsEnabled(false);
    }

    public override void ProcessHit(float damage)
    {
        base.ProcessHit(damage);
    }
}
