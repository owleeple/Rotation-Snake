using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxesController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 dir, float distance)
    {
        gameObject.transform.position = gameObject.transform.position + dir.normalized * distance;
    }
}
