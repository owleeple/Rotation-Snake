using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.zero;
    public List<Vector3> currentDirections;
    public List<GameObject> segments;

    public IEnumerator SnakeMove(Vector3 movedirection)
    {
        throw new NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
