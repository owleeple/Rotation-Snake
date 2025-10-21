using System.Collections.Generic;
using UnityEngine;

public class LevelManagerV2 : MonoBehaviour
{

    public SnakeController snakeController;
    private Vector3 inputDirection = Vector3.zero; // ????
    //private Vector3 moveDirection = Vector3.zero;
    private bool acceptInput = true;
    private bool turnAround = false;
    private bool rotation = false;
    private int frameCount = -2;



    private bool straitWalk = false;
    private bool isMoving = false;
    private bool portalCheck = false;
    private int segmentsOfMove = 0;

    // public int numberOfVerticalSlice = 5;
    // public int numberOfHorizontalSlice = 10;
    public Material material;
    // private float bendRadius = 0.5f;
    // private int bendSegments = 12;
    public int speed = 2;
    public float angle;
    private List<(Vector3 pivot, Vector3 axis)> pivotAndAxis;
    private List<GameObject> rotateGameobjectlist;
    private List<GameObject> frontobjects;
    private List<GameObject> boxesColorChanged;
    private LayerMask colorStripLayer;
    private LayerMask portalColliderLayer;
    private LayerMask colliderLayer;
    private LayerMask TranslateLayer;
    private LayerMask BoxLayer;
    private LayerMask PlayerLayer;
    private LayerMask PipeLayer;
    private LayerMask BoxPlayerLayer;
    private LayerMask MovementLayer;


    // portal movement
    public GameObject portalA;
    public GameObject portalB;
    private Dictionary<GameObject, GameObject> portal;

    // Start is called before the first frame update
    void Start()
    {
        pivotAndAxis = new List<(Vector3 pivot, Vector3 axis)>();
        frontobjects = new List<GameObject>();
        boxesColorChanged = new List<GameObject>();
        colorStripLayer = LayerMask.GetMask("ColorStrip");
        portalColliderLayer = LayerMask.GetMask("Box", "Player");
        colliderLayer = LayerMask.GetMask("Box", "Player", "Wall", "Pipe");
        // Movement is the layer of parent of segments of pipe that is different from segments of pipe.
        // Movement layer is created for the process of rotating pipe.
        TranslateLayer = LayerMask.GetMask("Box", "Player", "Wall", "Movement");
        BoxLayer = LayerMask.GetMask("Box");
        PlayerLayer = LayerMask.GetMask("Player");
        PipeLayer = LayerMask.GetMask("Pipe");
        BoxPlayerLayer = LayerMask.GetMask("Box", "Player");
        MovementLayer = LayerMask.GetMask("Movement");

        portal = new Dictionary<GameObject, GameObject>();
        portal.Add(portalA, portalB);
        portal.Add(portalB, portalA);

        rotateGameobjectlist = new List<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {
        if (snakeController.moveDirection != Vector3.zero) return;
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


        if (acceptInput && inputDirection != Vector3.zero)
        {
            snakeController.moveDirection = inputDirection;
            inputDirection = Vector3.zero;
            acceptInput = false;
            // direction is backward, snake will move the whole body backward.
            if (Vector3.Dot(snakeController.moveDirection, snakeController.currentDirections[1]) < -0.5)
            {
   


            }
            else
            {
                Collider2D col = snakeController.segments[0].GetComponent<Collider2D>();
                // Detection of obstatcle in front
                Vector2 dectectPos = col.bounds.center + snakeController.moveDirection * 0.8f;
                // here do not detect layer of segments of pipe but parent of them.
                Collider2D colrotate = Physics2D.OverlapCircle(dectectPos, 0.25f, TranslateLayer);

                if (colrotate == null)
                {
                    // Detection of pipe walls. pipepos is the positions of pipewall.
                    Vector2 pipeWallPos = col.bounds.center + snakeController.moveDirection * 0.5f;
                    Collider2D colpipe = Physics2D.OverlapCircle(pipeWallPos, 0.3f, MovementLayer);
                    if (colpipe != null)
                    {

                    }
                }
                else if (colrotate.gameObject.CompareTag("Wall") || colrotate.gameObject.CompareTag("Player"))
                {
                    acceptInput = true;
                    snakeController.moveDirection = Vector3.zero;
                    return;
                }
                else
                {
                    rotateGameobjectlist.Clear();
                    // Physics2D.OverlapCircle(dectectPos, 0.25f,TranslateLayer) has detected parent of segments of pipe
                    if (colrotate.CompareTag("Pipe"))
                    {
                        GetAllRotateGameobjects(colrotate.gameObject, rotateGameobjectlist);
                    }
                    else
                    {
                        GetAllRotateGameobjects(colrotate.transform.parent.gameObject, rotateGameobjectlist);
                    }

                    if (rotateGameobjectlist.Count != 0)
                    {
                        CaculateRotatePivotAndAxis(snakeController.moveDirection, pivotAndAxis, rotateGameobjectlist);
                        rotation = true;
                    }
                    else
                    {
                        acceptInput = true;
                        snakeController.moveDirection = Vector3.zero;
                        return;
                    }
                }








                if (Vector3.Dot(snakeController.moveDirection, snakeController.currentDirections[1]) < 0.5)
                {

                }
                else
                {
                    // strait walk
                    straitWalk = true;

                }




            }
        }






        if (isMoving)
        {

        }

        // if colliding with something after rotation,destroy it and it do not portalcheck.
        if (rotation)
        {
   
        }

        if (portalCheck && !rotation)
        {
            //process portal. if gm is in position of portal do something.
            // 1.process boxes and snake. update positions and directions.
            foreach (var pair in portal)
            {
                GameObject key = pair.Key;
                GameObject value = pair.Value;
                Collider2D col = Physics2D.OverlapCircle(key.transform.position, 0.3f, portalColliderLayer);
                Vector3 offset = value.transform.position - key.transform.position - snakeController.moveDirection;
                Vector3 dir = offset.normalized;
                float distance = offset.magnitude;
                if (col != null && col.CompareTag("Box"))
                {
                    GameObject parent = col.transform.parent.gameObject;

                    parent.GetComponent<BoxesController>().Move(dir, distance);
                }
                else if (col != null && col.CompareTag("Player"))
                {
        
                }
            }

            //end ,set portalCheck and snakerender.moveDirection
            portalCheck = false;
            snakeController.moveDirection = Vector3.zero;
        }



    }

    /// <summary>
    /// gameobject is geometricObject , parent of segments of pipe or player. rotates save all objects that possible be pipe or
    /// geometricObject for rotating later.   geometry--pipe--geometry--pipe...
    /// </summary>
    /// <param name="gm"></param>
    /// <param name="rotates"></param>
    private void GetAllRotateGameobjects(GameObject gameobject, List<GameObject> rotatingGameobjects)
    {

        rotatingGameobjects.Add(gameobject);
        Queue<GameObject> visiting = new Queue<GameObject>();
        visiting.Enqueue(gameobject);
        do
        {
            GameObject parent = visiting.Dequeue();
            if (parent.CompareTag("Box"))
            {
                foreach (Transform child in parent.transform)
                {
                    Vector2 pos = child.GetComponent<BoxCollider2D>().bounds.center;
                    // only detecting the segameobjectents of pipe
                    Collider2D col = Physics2D.OverlapCircle(pos, 0.2f, PipeLayer);
                    if (col != null && !rotatingGameobjects.Contains(col.transform.parent.gameObject))
                    {
                        rotatingGameobjects.Add(col.transform.parent.gameObject);
                        visiting.Enqueue(col.transform.parent.gameObject);
                    }
                }
            }
            else
            {
                // pipe object process

                foreach (Transform child in parent.transform)
                {
                    Vector2 pos = child.GetComponent<BoxCollider2D>().bounds.center;
                    // do detect Layer of pipe itself
                    Collider2D col = Physics2D.OverlapCircle(pos, 0.2f, BoxPlayerLayer);
                    if (col != null && col.CompareTag("Player"))
                    {
                        rotatingGameobjects.Clear();
                        return;
                    }

                    if (col != null && !rotatingGameobjects.Contains(col.transform.parent.gameObject))
                    {
                        rotatingGameobjects.Add(col.transform.parent.gameObject);
                        visiting.Enqueue(col.transform.parent.gameObject);
                    }
                }

            }

        } while (visiting.Count != 0);
    }

    private void CaculateRotatePivotAndAxis(Vector3 dir, List<(Vector3 pivot,Vector3 axis)> pivotAndAxis, List<GameObject> rotatingGeometries)
    {
        pivotAndAxis.Clear();
        Transform targetChild = rotatingGeometries[0].transform;
        float bestDot = float.MinValue; // ??
        for (int i = 0; i < rotatingGeometries.Count; i++)
        {
            GameObject geometry = rotatingGeometries[i];
            foreach (Transform child in geometry.transform)
            {
                float dot = Vector3.Dot(child.position, dir);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    targetChild = child;
                }
            }

            // targetChild.position is not the farthest point. it need add half length of itself.
            Vector3 halfLengthOfChild = targetChild.localScale * 0.5f;
            Vector3 edgeOffset = Vector3.Scale(dir.normalized, halfLengthOfChild);
            Vector3 pivot = targetChild.position + edgeOffset;
            Vector3 axis = Quaternion.AngleAxis(90f, Vector3.back) * dir;
            pivotAndAxis.Add((pivot,axis));
        }
    }

    // consider if it is movable according to frontobjects
    public bool CanMoveBackward(Vector2 dir, GameObject detectobject, List<GameObject> frontobjects)
    {
        // ??????? null ??
        if (frontobjects == null)
        {
            Debug.LogWarning("frontobjects is null");
            return false; // ?? true?????????
        }

        Queue<GameObject> checkObjects = new Queue<GameObject>();
        checkObjects.Enqueue(detectobject);
        do
        {
            GameObject checking = checkObjects.Dequeue();
            if (checking.transform.childCount == 0)
            {
                Debug.Log("cheking do have child, this is a error--------------------------------");
                Debug.Log(checking.name);
            }

            if (checking.CompareTag("Pipe"))
            {
                BoxCollider2D[] pipeColliders = checking.GetComponents<BoxCollider2D>();
                int borderIndex = checking.GetComponent<ColliderData>().borderIndex;
                for (int i = 0; i < pipeColliders.Length - 1; i++)
                {

                    if (i == borderIndex) continue;
                    Vector2 start = (Vector2)pipeColliders[i].bounds.center + dir * 0.5f;
                    Vector2 end = (Vector2)pipeColliders[i + 1].bounds.center + dir * 0.5f;
                    Collider2D[] frontColliders = GetCollidersBetweenTwoPoint(start, end, TranslateLayer);
                    foreach (Collider2D frontCollider in frontColliders)
                    {
                        if (frontCollider.CompareTag("Wall"))
                        {
                            return false;
                        }
                        else if (frontCollider.CompareTag("Player"))
                        {
                            continue;
                        }
                        else if (frontCollider.CompareTag("Box"))
                        {
                            if (frontobjects.Contains(frontCollider.transform.parent.gameObject))
                            {
                                continue;
                            }
                            else
                            {
                                checkObjects.Enqueue(frontCollider.transform.parent.gameObject);
                                frontobjects.Add(frontCollider.transform.parent.gameObject);
                            }

                        }
                        else if (frontCollider.gameObject == checking.gameObject)
                        {
                            continue;
                        }
                        else if (frontobjects.Contains(frontCollider.gameObject))
                        {
                            continue;
                        }
                        else
                        {
                            checkObjects.Enqueue(frontCollider.gameObject);
                            frontobjects.Add(frontCollider.gameObject);
                        }
                    }
                }

                for (int i = 0; i < pipeColliders.Length; i++)
                {
                    Vector2 start = (Vector2)pipeColliders[i].bounds.center;
                    Vector2 end = (Vector2)pipeColliders[i].bounds.center + dir;
                    Collider2D[] frontColliders = GetCollidersBetweenTwoPoint(start, end, TranslateLayer);
                    foreach (Collider2D frontCollider in frontColliders)
                    {
                        if (frontCollider.CompareTag("Wall"))
                        {
                            return false;
                        }
                        else if (frontCollider.CompareTag("Player"))
                        {
                            continue;
                        }
                        else if (frontCollider.CompareTag("Box"))
                        {
                            if (frontobjects.Contains(frontCollider.transform.parent.gameObject))
                            {
                                continue;
                            }
                            else
                            {
                                checkObjects.Enqueue(frontCollider.transform.parent.gameObject);
                                frontobjects.Add(frontCollider.transform.parent.gameObject);
                            }

                        }
                        else if (frontCollider.gameObject == checking.gameObject)
                        {
                            continue;
                        }
                        else if (frontobjects.Contains(frontCollider.gameObject))
                        {
                            continue;
                        }
                        else
                        {
                            checkObjects.Enqueue(frontCollider.gameObject);
                            frontobjects.Add(frontCollider.gameObject);
                        }
                    }
                }


            }
            else
            {
                for (int i = 0; i < checking.transform.childCount - 1; i++)
                {
                    Transform child = checking.transform.GetChild(i);
                    Transform childnext = checking.transform.GetChild(i + 1);
                    Collider2D col = child.GetComponent<Collider2D>();
                    Collider2D colnext = childnext.GetComponent<Collider2D>();
                    Vector2 start = (Vector2)col.bounds.center + dir;
                    Vector2 end = (Vector2)colnext.bounds.center + dir;



                    Collider2D[] frontColliders = GetCollidersBetweenTwoPoint(start, end, TranslateLayer);
                    foreach (Collider2D frontCollider in frontColliders)
                    {
                        if (frontCollider.CompareTag("Wall"))
                        {
                            return false;
                        }
                        else if (frontCollider.CompareTag("Player"))
                        {
                            continue;
                        }
                        else if (frontCollider.CompareTag("Pipe"))
                        {
                            if (frontobjects.Contains(frontCollider.gameObject))
                            {
                                continue;
                            }
                            else
                            {
                                checkObjects.Enqueue(frontCollider.gameObject);
                                frontobjects.Add(frontCollider.gameObject);
                            }

                        }
                        else if (frontCollider.transform.parent == child.parent)
                        {
                            continue;
                        }
                        else if (frontobjects.Contains(frontCollider.transform.parent.gameObject))
                        {
                            continue;
                        }
                        else
                        {
                            checkObjects.Enqueue(frontCollider.transform.parent.gameObject);
                            frontobjects.Add(frontCollider.transform.parent.gameObject);
                        }
                    }

                }

                // forward path dectect
                foreach (Transform child in checking.transform)
                {
                    Collider2D col = child.GetComponent<Collider2D>();
                    Vector2 pos = col.bounds.center;
                    Vector2 posnext = pos + dir;
                    Collider2D[] frontColliders = GetCollidersBetweenTwoPoint(pos, posnext, TranslateLayer);
                    foreach (Collider2D frontCollider in frontColliders)
                    {
                        if (frontCollider.CompareTag("Wall"))
                        {
                            return false;
                        }
                        else if (frontCollider.CompareTag("Player"))
                        {
                            continue;
                        }
                        else if (frontCollider.CompareTag("Pipe"))
                        {
                            if (frontobjects.Contains(frontCollider.gameObject))
                            {
                                continue;
                            }
                            else
                            {
                                checkObjects.Enqueue(frontCollider.gameObject);
                                frontobjects.Add(frontCollider.gameObject);
                            }
                        }
                        else if (frontCollider.transform.parent == child.parent)
                        {
                            continue;
                        }
                        else if (frontobjects.Contains(frontCollider.transform.parent.gameObject))
                        {
                            continue;
                        }
                        else
                        {
                            checkObjects.Enqueue(frontCollider.transform.parent.gameObject);
                            frontobjects.Add(frontCollider.transform.parent.gameObject);
                        }
                    }
                }
            }







        } while (checkObjects.Count != 0);

        return true;

    }

    /*    private Vector3 GetDirectionOfFirstOrLast(Transform child)
        {
            int index = child.GetSiblingIndex();
            if(index == 0)
            {
                return 
            }
        }*/

    public static Collider2D[] GetCollidersBetweenTwoPoint(Vector2 start, Vector2 end, LayerMask layerMask = default)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        if (distance < Mathf.Epsilon)
            return new Collider2D[0];

        // ??RaycastAll??????
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, direction.normalized, distance, layerMask);

        // ?????
        Collider2D[] colliders = new Collider2D[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            colliders[i] = hits[i].collider;
        }

        return colliders;
    }

    private bool IsFirstOrLastChild(Transform child)
    {
        // ?? child ??? null
        if (child == null)
        {
            Debug.LogWarning("Child is null");
            return false;
        }

        // ?? child ??????
        if (child.parent == null)
        {
            Debug.LogWarning("Child has no parent");
            return false;
        }

        Transform parent = child.parent;

        // ???????????
        if (parent.childCount == 0)
        {
            Debug.LogWarning("Parent has no children");
            return false;
        }

        // ?????????????
        int childIndex = child.GetSiblingIndex();

        // ????????????????
        bool isFirstChild = (childIndex == 0);
        bool isLastChild = (childIndex == parent.childCount - 1);

        return isFirstChild || isLastChild;
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
                if (minheight > 100) minheight = 100;
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

        Vector2 difference = endPoint - startPoint;
        float distance = Mathf.Sqrt(difference.x * difference.x + difference.y * difference.y);
        return Mathf.RoundToInt(distance);
    }



}
