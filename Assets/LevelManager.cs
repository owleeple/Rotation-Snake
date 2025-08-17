using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{


    private Vector3 inputDirection = Vector3.zero; // ????
    private Vector3 snakeDirection = Vector3.zero;
    private bool canMove = true;
    private bool rotation = false;
    private int frameCount = 0;

    public int numberOfVerticalSlice = 5;
    public int numberOfHorizontalSlice = 10;
    public Material material;
    // private float bendRadius = 0.5f;
    // private int bendSegments = 12;
    public int speed = 2;
    public float angle;
    private List<Vector3> rotateList;

    // Start is called before the first frame update
    void Start()
    {
        angle = 180 / numberOfHorizontalSlice;
        rotateList = new List<Vector3>();
        rotateList.Add(Vector3.zero);
        rotateList.Add(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        if (snakeDirection != Vector3.zero) return;
        // ?????
        if (Input.GetKeyDown(KeyCode.W))
            inputDirection = Vector3.up;
        if (Input.GetKeyDown(KeyCode.S))
            inputDirection = Vector3.down;
        if (Input.GetKeyDown(KeyCode.A))
            inputDirection = Vector3.left;
        if (Input.GetKeyDown(KeyCode.D))
            inputDirection = Vector3.right;

        // ?????
        if (Input.GetKeyUp(KeyCode.W) && inputDirection == Vector3.up)
            inputDirection = Vector3.zero;
        if (Input.GetKeyUp(KeyCode.S) && inputDirection == Vector3.down)
            inputDirection = Vector3.zero;
        if (Input.GetKeyUp(KeyCode.A) && inputDirection == Vector3.left)
            inputDirection = Vector3.zero;
        if (Input.GetKeyUp(KeyCode.D) && inputDirection == Vector3.right)
            inputDirection = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (canMove && inputDirection != Vector3.zero)
        {
            canMove = false;
            snakeDirection = inputDirection;
            inputDirection = Vector3.zero;
            rotation = true;
            GameObject gm = GameObject.Find("GeometricObject");
            CaculateRotatePivotAndAxis(snakeDirection, rotateList, gm);
        }

        if (rotation)
        {
            frameCount++; 
            if(frameCount % speed == 0)
            {
                int numberOfSchedule = frameCount / speed;
                if (numberOfSchedule <= numberOfHorizontalSlice)
                {
                   // float angles = angle * numberOfSchedule;
                    GameObject gm = GameObject.Find("GeometricObject");
                    Vector3 pivot = rotateList[0];
                    Vector3 axis = rotateList[1];
                    gm.transform.RotateAround(pivot, axis, angle);
                }
                else
                {
                    rotation = false;
                    canMove = true;
                    snakeDirection = Vector3.zero;
                    frameCount = 0;
                }
            }
        }


    }

    private void CaculateRotatePivotAndAxis(Vector3 dir, List<Vector3> list, GameObject gm)
    {
        if (gm.transform.childCount == 0) return;

        // 1. ??? dir ?????????
        Transform targetChild = gm.transform.GetChild(0);
        float bestDot = Vector3.Dot(targetChild.position, dir); // ??
        foreach (Transform child in gm.transform)
        {
            float dot = Vector3.Dot(child.position, dir);
            if (dot > bestDot) // ???Vector3.left ???????“?”
            {
                bestDot = dot;
                targetChild = child;
            }
        }

        // 2. ?????????????pivot ????
        Vector3 halfExtents = targetChild.localScale * 0.5f;
        Vector3 edgeOffset = Vector3.Scale(dir.normalized, halfExtents);
        Vector3 pivot = targetChild.position + edgeOffset;
        list[0] = pivot;

        // 3. ?????
        // ???? 2D ???? Z ??????? 3D???????
        Vector3 axis = Quaternion.AngleAxis(90f, Vector3.back) * dir;
        list[1] = axis;
        
        
    }





}
