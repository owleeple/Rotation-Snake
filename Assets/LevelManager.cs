using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public SnakeRender snakeRender;
    private Vector3 inputDirection = Vector3.zero; // ????
    //private Vector3 snakeDirection = Vector3.zero;
    private bool canMove = true;
    private bool turnAround = false;
    private bool rotation = false;
    private int frameCount = 0;

    

    private bool straitWalk = false;
    private int segmentsOfMove = 0;

   // public int numberOfVerticalSlice = 5;
   // public int numberOfHorizontalSlice = 10;
    public Material material;
    // private float bendRadius = 0.5f;
    // private int bendSegments = 12;
    public int speed = 2;
   // public float angle;
    private List<Vector3> rotateList;

    // Start is called before the first frame update
    void Start()
    {
        /*  angle = 180 / numberOfHorizontalSlice;
          rotateList = new List<Vector3>();
          rotateList.Add(Vector3.zero);
          rotateList.Add(Vector3.zero);*/
/*
        // test GetHeight
        GameObject gm = GameObject.Find("Snake");
        Dictionary<GameObject, int> hmap = new Dictionary<GameObject, int>();
        Vector2 dir = Vector2.down;
        HashSet<GameObject> visited = new HashSet<GameObject>();
        int height = GetHeight(gm, hmap, dir, visited);
        Debug.Log("height is 4");
        Debug.Log("REAL IS " + height);*/
    }

    // Update is called once per frame
    void Update()
    {
        if (snakeRender.snakeDirection != Vector3.zero) return;
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
            snakeRender.snakeDirection = inputDirection;
            inputDirection = Vector3.zero;
            canMove = false;
            if (Vector3.Dot(snakeRender.snakeDirection, snakeRender.currentDirections[1]) < 0.5)
            {

                turnAround = true;
                snakeRender.backupVerticesOfhead = snakeRender.meshes[0].vertices;
                snakeRender.UpdateTargetPosition();
                // process second segment


                snakeRender.SetDataOfSecondSegmentForTurnAround();

                snakeRender.SetDataOfTail();





            }
            else
            {
                // strait walk

                straitWalk = true;
                snakeRender.UpdateTargetPosition();
                snakeRender.SetDataOfSecondSegmentForStraitWalk();


                // 2.process tail segment
                snakeRender.SetDataOfTail();


            }
        }



        if (turnAround)
        {
            segmentsOfMove += snakeRender.count;
            if (segmentsOfMove % speed == 0)
            {
                if (segmentsOfMove / speed <= snakeRender.numberOfHorizontalSlice)
                {
                    int number = segmentsOfMove / speed;

                    snakeRender.UpdateSegmentsInMiddleForTurnAround(number);
                    snakeRender.UpdateHeadForTurnAround(number);

                    snakeRender.UpdateTail();



                }
                else
                {
                    turnAround = false;
                    canMove = true;
                    snakeRender.snakeDirection = Vector3.zero;
                    segmentsOfMove = 0;
                    snakeRender.UpdateCurrentPositionAndCurrentDirection();
                }
            }

        }


        if (straitWalk)
        {
            segmentsOfMove += snakeRender.count;
            if (segmentsOfMove % speed == 0)
            {
                if (segmentsOfMove / speed <= snakeRender.numberOfHorizontalSlice)
                {
                    int number = segmentsOfMove / speed;
                    snakeRender.UpdateSegmentsInMiddleForStraitWalk(number);

                    snakeRender.UpdateHeadForStraitWalk();

                    snakeRender.UpdateTail();

                }
                else
                {
                    // 
                    straitWalk = false;
                    canMove = true;
                    snakeRender.snakeDirection = Vector3.zero;
                    segmentsOfMove = 0;
                    snakeRender.UpdateCurrentPositionAndCurrentDirection();

                }
            }
        }





        /*if (canMove && inputDirection != Vector3.zero)
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
        }*/


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

    // if gameobject can not hit anything, height will be 100
    public int GetHeight(GameObject parent, Dictionary<GameObject, int> heightMap, Vector2 direction, HashSet<GameObject> visited)
    {
        if (heightMap.ContainsKey(parent))
        {
            return heightMap[parent];
        }
        
        visited.Add(parent);

        int minheight = int.MaxValue;
        foreach (Transform child in parent.transform)
        {
            int childheight = int.MaxValue;
            Vector2 startpos = child.gameObject.GetComponent<Collider2D>().bounds.center;
            RaycastHit2D hit = Physics2D.Raycast(startpos + direction, direction);
            if (hit.collider != null)
            {
                Vector2 hitpos = hit.collider.bounds.center;
                int hitDistance = GetDistance(startpos, hitpos) - 1;
                if (hit.collider.CompareTag("Ground"))
                {

                    childheight = hitDistance;

                }
                else
                {
                    Transform hitParent = hit.collider.transform.parent;
                    if (hitParent != null && hitParent == parent.transform)
                    {
                        continue;
                    }
                    if (visited.Contains(hit.collider.gameObject)) continue;
                    if (hit.collider.transform.parent == null)
                    {
                        Debug.Log("hit.collider do not have parent gameobject");
                    }
                    GameObject gm = hit.collider.transform.parent.gameObject;
                    childheight = GetHeight(gm, heightMap, direction, visited) + hitDistance;
                }

                if (childheight < minheight) minheight = childheight;

            }
            else
            {                             
                if(minheight > 100) minheight = 100;            
            }
        }

        if (minheight != int.MaxValue)
        {
            heightMap[parent] = minheight;
        }
        return minheight;
    }


    private int GetDistance(Vector2 startPoint, Vector2 endPoint)
    {
        // ???????????
        Vector2 difference = endPoint - startPoint;

        // ??????????????
        float distance = Mathf.Sqrt(difference.x * difference.x + difference.y * difference.y);

        // ??????????????????int?
        return Mathf.RoundToInt(distance);
    }



}
