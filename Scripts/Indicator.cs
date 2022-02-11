using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 90f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        var mousePos = FindObjectOfType<LookAtMouse>().GetMousePos();
        var newPos = new Vector3(mousePos.x, 0.01f, mousePos.z);
        transform.position = newPos;
    }
}
