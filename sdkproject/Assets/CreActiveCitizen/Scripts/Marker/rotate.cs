using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //changeRotate();
        float steerAngleX = (transform.eulerAngles.x < 180f) ? transform.eulerAngles.x : transform.eulerAngles.x - 360;
        float steerAngleY = (transform.eulerAngles.y < 180f) ? transform.eulerAngles.y : transform.eulerAngles.y - 360;
        float steerAngleZ = (transform.eulerAngles.z < 180f) ? transform.eulerAngles.z : transform.eulerAngles.z - 360;
        Debug.Log(steerAngleX);
        Debug.Log(steerAngleY);
        Debug.Log(steerAngleZ);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void changeRotate()
    {
       transform.rotation = Quaternion.Euler(0, 45, 0);
    }
}
