using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionCube : Hazard
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ProcessHit(float damage)
    {
        Debug.Log("Minion Cube Immune");
    }
}
