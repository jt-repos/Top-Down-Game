using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    Vector3 mousePos;
    Vector3 lookDir;
    LayerMask indicatorPlane;
    RaycastHit hit;
    bool isOn = true;

    // Start is called before the first frame update
    void Start()
    {
        indicatorPlane = LayerMask.GetMask("IndicatorHit");
    }

    // Update is called once per frame
    void Update()
    {
        if(isOn)
        {
            var cameraRotation = mainCamera.transform.localRotation * Vector3.forward;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f, indicatorPlane))
            {
                mousePos = hit.point;
                lookDir = new Vector3(mousePos.x, transform.position.y, mousePos.z);
            }
            transform.LookAt(lookDir);
        }
    }

    public Vector3 GetMousePos()
    {
        return mousePos;
    }

    public void IsOn(bool value)
    {
        isOn = value;
    }
}
