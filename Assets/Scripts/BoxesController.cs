using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class BoxesController : MonoBehaviour
{
    private float speed = 4;
    private Vector3 targetPosition;
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

    public IEnumerator BoxTranslate(Vector3 moveDirection)
    {
        float frameSpeed = speed / 50;
        targetPosition = transform.position + moveDirection;
        
        while ((transform.position - targetPosition).magnitude > frameSpeed)
        {
          transform.position = transform.position + frameSpeed * moveDirection;
          yield return new WaitForFixedUpdate();
        }
        transform.position = targetPosition;
    }

    public void SetSpeed(float speedvalue)
    {
        speed = speedvalue;
    }

    public IEnumerator BoxRotation(Vector3 moveDirection, Vector3 pivot, Vector3 axis)
    {
        float angle = speed * 180 / 50;
        float curAngle = 0;
        while (Math.Abs(curAngle - 180) > angle)
        {
            transform.RotateAround(pivot, axis, angle);
            curAngle += angle;
            yield return new WaitForFixedUpdate();
        }
        // adjust to 180.
        transform.RotateAround(pivot, axis, 180 - curAngle);
      
    }
}
