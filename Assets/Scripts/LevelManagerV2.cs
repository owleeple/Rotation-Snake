using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class LevelManagerV2 : MonoBehaviour
{

    public SnakeController snakeController;
    public float speed = 4;
    public float angle;
    private Vector3 inputDirection = Vector3.zero; // ????
    //private Vector3 moveDirection = Vector3.zero;
  
    private bool isMoving = false;
    private bool startMove = false;
    private bool startRotate = false;
    private bool isRotating = false;
    private bool startTranslate = false;
    private bool isTranslating = false;
  //  private bool portalCheck = false;

  // check objects that is push by snake.
    private Queue<GameObject> visitingGameObjects;
    private List<GameObject> translateGameobjectlist;
    private List<GameObject> rotateGameobjectlist;
    private List<(Vector3 pivot, Vector3 axis)> pivotAndAxis;
    // public Material material;



    private List<GameObject> frontobjects;
    private List<GameObject> roastingBoxes;
    private LayerMask colorStripLayer;
    private LayerMask portalColliderLayer;
    private LayerMask colliderLayer;
    private LayerMask TranslateLayer;
    private LayerMask BoxLayer;
    private LayerMask PlayerLayer;
    private LayerMask PipeLayer;
    private LayerMask BoxPlayerLayer;
    private LayerMask MovementLayer;


    private List<Coroutine> boxTranslateCoroutines;
    private List<Coroutine> boxRotationCoroutines;
    private bool IsRebound;
    private Vector3 ReboundDirection = Vector3.zero;

    // portal movement
    /*    public GameObject portalA;
        public GameObject portalB;
        private Dictionary<GameObject, GameObject> portal;*/

    // Start is called before the first frame update
    void Start()
    {
        pivotAndAxis = new List<(Vector3 pivot, Vector3 axis)>();
        frontobjects = new List<GameObject>();
        roastingBoxes = new List<GameObject>();
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

/*        portal = new Dictionary<GameObject, GameObject>();
        portal.Add(portalA, portalB);
        portal.Add(portalB, portalA);*/

        rotateGameobjectlist = new List<GameObject>();
        translateGameobjectlist = new List<GameObject>();
        visitingGameObjects = new Queue<GameObject>();
        boxTranslateCoroutines = new List<Coroutine>();
        boxRotationCoroutines = new List<Coroutine>();

        // synchronize the speed of snake and all boxes
        snakeController.SetSpeed(speed);
        BoxesController[] allBoxControllers = FindObjectsOfType<BoxesController>();
        foreach (BoxesController boxController in allBoxControllers)
        {
            boxController.SetSpeed(speed);
        }
    }

    // Update is called once per frame;
    void Update()
    {
        if (isMoving || isRotating || isTranslating) return;
        Vector3 newDirection = Vector3.zero;

        // ???????????????????
        if (Input.GetKey(KeyCode.W))
            newDirection = Vector3.up;
        else if (Input.GetKey(KeyCode.S))
            newDirection = Vector3.down;
        else if (Input.GetKey(KeyCode.A))
            newDirection = Vector3.left;
        else if (Input.GetKey(KeyCode.D))
            newDirection = Vector3.right;

        // ????????????????
        if (newDirection != Vector3.zero && newDirection != inputDirection)
        {
            inputDirection = newDirection;
        }
    }

    private void FixedUpdate()
    {
        Console.OutputEncoding = Encoding.UTF8;
        //center process
        if (inputDirection != Vector3.zero && !isMoving && !isRotating)
        {
            visitingGameObjects.Clear();
            rotateGameobjectlist.Clear();
            translateGameobjectlist.Clear();
            GameObject snakeHead = snakeController.GetHead();
            visitingGameObjects.Enqueue(snakeHead);
            if (!CanMoveForward(inputDirection, visitingGameObjects, translateGameobjectlist, rotateGameobjectlist)) {
                inputDirection = Vector3.zero;
                return; 
            }
            
            // process snake movement
            snakeController.moveDirection = inputDirection;
            inputDirection = Vector3.zero;
            startMove = true;
            isMoving = true;
            // process rotation
            if (rotateGameobjectlist.Count != 0)
            {
                isRotating = true;
                startRotate = true;
            }
            // process translate.
            if(translateGameobjectlist.Count != 0)
            {
                // set the direction of roasted boxes that will become roasted color
                for (int i = 0; i < translateGameobjectlist.Count; i++)
                {
                    foreach (Transform child in translateGameobjectlist[i].transform)
                    {
                        Vector2 centerOfGridEdge = child.position + snakeController.moveDirection * 0.5f;
                        Collider2D colorStrip = Physics2D.OverlapCircle(centerOfGridEdge, 0.3f, colorStripLayer);
                        if (colorStrip != null && colorStrip.CompareTag("ColorVariation"))
                        {
                            roastingBoxes.Add(child.gameObject);
                            child.gameObject.GetComponent<BoxColorController>().SetDirection(snakeController.moveDirection);
                        }
                    }
                }
                isTranslating = true;
                startTranslate = true;
               
            }
            
            // 
        }


        if (startMove)
        {
            startMove = false;
            StartCoroutine(SnakeMove(snakeController.moveDirection));
        }


        if (startRotate)
        {
            startRotate = false;
            if (Vector2.Dot(snakeController.moveDirection, Vector2.right) != 0.0f)
            {
                for (int i = 0; i < rotateGameobjectlist.Count; i++)
                {
                    foreach (Transform child in rotateGameobjectlist[i].transform)
                    {
                        child.gameObject.GetComponent<BoxColorController>().ReverseHorizontalRotation();

                    }
                }
            }
            else
            {
                for (int i = 0; i < rotateGameobjectlist.Count; i++)
                {
                    foreach (Transform child in rotateGameobjectlist[i].transform)
                    {
                        child.gameObject.GetComponent<BoxColorController>().ReverseVerticalRotation();
                    }
                }
            }
            CaculateRotatePivotAndAxis(snakeController.moveDirection, pivotAndAxis, rotateGameobjectlist);
            StartCoroutine(BoxRotation(snakeController.moveDirection, pivotAndAxis, rotateGameobjectlist));
        }

        if (startTranslate)
        {
            startTranslate = false;
            StartCoroutine(BoxTranslate(snakeController.moveDirection,translateGameobjectlist,roastingBoxes));
        }

  /*      if (IsRebound)
        {
            pivotAndAxis.Clear();
            CaculateRotatePivotAndAxis(ReboundDirection, pivotAndAxis, rotateGameobjectlist);
            StartCoroutine(GeometryRebound(ReboundDirection, pivotAndAxis, rotateGameobjectlist));
        }*/

    }

    private IEnumerator GeometryRebound(Vector3 moveDirection, List<(Vector3 pivot, Vector3 axis)> pivotAndAxis, List<GameObject> rotateGeometries)
    {
        boxRotationCoroutines.Clear();
        for (int i = 0; i < rotateGeometries.Count; i++)
        {
            var box = rotateGeometries[i];
            var (pivot, axis) = pivotAndAxis[i];
            Coroutine boxtranslateCoroutine = StartCoroutine(box.GetComponent<BoxesController>().BoxRotation(moveDirection, pivot, axis));
            boxRotationCoroutines.Add(boxtranslateCoroutine);
        }

        for (int i = 0; i < boxRotationCoroutines.Count; i++)
        {
            yield return boxRotationCoroutines[i];
        }
        IsRebound = false;

    }

    private IEnumerator BoxRotation(Vector3 moveDirection, List<(Vector3 pivot, Vector3 axis)> pivotAndAxis, List<GameObject> rotateGeometries)
    {
        boxRotationCoroutines.Clear();
        for (int i = 0; i < rotateGeometries.Count; i++)
        {
            var box = rotateGeometries[i];
            var (pivot, axis) = pivotAndAxis[i];
            Coroutine boxtranslateCoroutine = StartCoroutine(box.GetComponent<BoxesController>().BoxRotation(moveDirection,pivot,axis));
            boxRotationCoroutines.Add(boxtranslateCoroutine);
        }

        for (int i = 0; i < boxRotationCoroutines.Count; i++)
        {
            yield return boxRotationCoroutines[i];
        }
        isRotating = false;
/*        if (IsGeometryRebound(rotateGeometries))
        {
            IsRebound = true;
            ReboundDirection = -moveDirection;
        }*/
    }

    private bool IsGeometryRebound(List<GameObject> rotateGeometries)
    {
        foreach (var box in rotateGeometries)
        {
            var collider = Physics2D.OverlapCircle(box.transform.position, 0.3f);
            if(collider.CompareTag("Wall") || collider.CompareTag("Player") || collider.CompareTag("Box"))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator BoxTranslate(Vector3 moveDirection,List<GameObject> translateBoxes, List<GameObject> roastingBoxes)
    {
        boxTranslateCoroutines.Clear();
        foreach (var box in translateBoxes)
        {
            Coroutine boxtranslateCoroutine = StartCoroutine(box.GetComponent<BoxesController>().BoxTranslate(moveDirection, roastingBoxes));
            boxTranslateCoroutines.Add(boxtranslateCoroutine);
        }

        for (int i = 0; i < boxTranslateCoroutines.Count; i++)
        {
            yield return boxTranslateCoroutines[i];
        }
        roastingBoxes.Clear();
        isTranslating = false;
    }

    private bool CanMoveForward(Vector3 inputDirection,Queue<GameObject> visiting_gameObjects, List<GameObject> translate_gameobject_list, List<GameObject> rotate_gameobject_list)
    {
        bool isFirstTranslated = false;
        do
        {
            GameObject gameobject = visiting_gameObjects.Dequeue();
            Collider2D frontObject = Physics2D.OverlapCircle(gameobject.transform.position + inputDirection, 0.3f, colliderLayer);
            if (frontObject == null) continue;
            if (frontObject.CompareTag("Wall")) { 
                return false;
            }
            if (frontObject.CompareTag("Player")) {
                return false; 
            }
            if (rotate_gameobject_list.Contains(frontObject.transform.parent.gameObject) ||
                translate_gameobject_list.Contains(frontObject.transform.parent.gameObject)) continue;
            if(frontObject.transform.parent == null)
            {
                throw new ArgumentNullException(nameof(frontObject.transform.parent.gameObject), "object can not be null");
            }

            if(isFirstTranslated)
            {
                translate_gameobject_list.Add(frontObject.transform.parent.gameObject);
                foreach (Transform child in frontObject.transform.parent)
                {
                    visiting_gameObjects.Enqueue(child.gameObject);
                }
                continue;
            }

            float border = GetBorderInThisDirection(frontObject.transform.parent, inputDirection);
            float difference = 0;
            if(inputDirection == Vector3.right || inputDirection == Vector3.left) {
                difference = Math.Abs(frontObject.transform.position.x - border);
            }
            else
            {
                difference = Math.Abs(frontObject.transform.position.y - border);
            }

            if(difference < 0.5)
            {
                rotate_gameobject_list.Add(frontObject.transform.parent.gameObject);
                continue;
            }
            else
            {
                translate_gameobject_list.Add(frontObject.transform.parent.gameObject);
                isFirstTranslated = true;
            }
            foreach (Transform child in frontObject.transform.parent)
            {
                visiting_gameObjects.Enqueue(child.gameObject);
            }


        } while (visitingGameObjects.Count != 0);
        return true;
    }

    private float GetBorderInThisDirection(Transform parent, Vector3 inputDirection)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent), "??????null");
        }

        if (parent.childCount == 0)
        {
            throw new InvalidOperationException("????????");
        }

        if (inputDirection == Vector3.zero)
        {
            throw new ArgumentException("??????????", nameof(inputDirection));
        }

        // ???????
        Vector3 normalizedDirection = inputDirection.normalized;

        // ???????????????
        bool isHorizontal = Mathf.Abs(normalizedDirection.x) > Mathf.Abs(normalizedDirection.y);
        Func<Transform, float> valueSelector;
        Func<float, float, bool> comparer;

        if (isHorizontal)
        {
            // ???????x?
            if (normalizedDirection.x > 0)
            {
                // ????x????
                valueSelector = child => child.position.x;
                comparer = (a, b) => a > b;
            }
            else
            {
                // ????x????
                valueSelector = child => child.position.x;
                comparer = (a, b) => a < b;
            }
        }
        else
        {
            // ???????y?
            if (normalizedDirection.y > 0)
            {
                // ????y????
                valueSelector = child => child.position.y;
                comparer = (a, b) => a > b;
            }
            else
            {
                // ????y????
                valueSelector = child => child.position.y;
                comparer = (a, b) => a < b;
            }
        }

        // ??????????????
        Transform farthestChild = parent.GetChild(0);
        float farthestValue = valueSelector(farthestChild);

        for (int i = 1; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            float currentValue = valueSelector(child);

            if (comparer(currentValue, farthestValue))
            {
                farthestValue = currentValue;
            }
        }

        return farthestValue;
    }

    private IEnumerator SnakeMove(Vector3 movedirection)
    {

        Coroutine snakeMove = StartCoroutine(snakeController.SnakeMove(snakeController.moveDirection));

        yield return snakeMove;
        isMoving = false;
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
