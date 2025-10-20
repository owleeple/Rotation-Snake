using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFunction : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 axis = Quaternion.AngleAxis(90f, Vector3.back) * Vector3.right;
        int a = 10;
       // Vector3.left.RotateAround(Vector3.zero, axis, 90);
    }
}
