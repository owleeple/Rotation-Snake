using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using static UnityEngine.UI.Image;

public class SnakeRender : MonoBehaviour
{
    public float snakeWidth = 0.8f;
    // Vertical segmentation
    public int numberOfVerticalSlice = 5;
    public int numberOfHorizontalSlice = 10;
    public Material material;
    // private float bendRadius = 0.5f;
    private int bendSegments = 12;
    public int speed = 2;
    private int length;
    private int indexOfvertex = 0;
    // set up coordinates for vertice of right order
    private List<Vector3> coordinates = new List<Vector3>();
    private List<int> shape = new List<int>();
    private int turn = 0;
    // create list of vertices for new algorithm that only update head and tail in list
    private List<List<Vector3>> verticesOfSegment = new List<List<Vector3>>();
    private List<List<Vector2>> uvsOfSegment = new List<List<Vector2>>();
    private List<List<int>> trianglesOfSegment = new List<List<int>>();
    private List<List<Vector3>> normalsOfSegment = new List<List<Vector3>>();

    private List<GameObject> segments;
    private List<Vector3> targetPositions;
    public List<Vector3> currentPositions;
    private List<Vector3> currentDirections;
    private List<Mesh> meshes;

    

   
    
    
    // private LinkedList<Vector3> forwardDirections;


    // Start is called before the first frame update
    void Start()
    {

        segments = new List<GameObject>();

        foreach (Transform child in transform)
        {
            segments.Add(child.gameObject);
        }

        // initial targetposition and currentdirection
         length = segments.Count;    
        targetPositions = new List<Vector3>(length);
        for (int i = 0; i < length; i++)
        {
            targetPositions.Add(Vector3.zero); 
        }
        currentDirections = new List<Vector3>(length);
        Vector3 headdir = currentPositions[0]-currentPositions[1];
        currentDirections.Add(headdir) ;
        for (int i = 1; i < length; i++)
        {    
            currentDirections.Add(currentPositions[i - 1] - currentPositions[i]);         
        }

        //initial meshes
        meshes = new List<Mesh>();
        for (int i = 0; i < length; i++)
        {
            meshes.Add(new Mesh());
        }

        // initial elements of mesh
        for (int i = 0; i < length; i++)
        {
           
            verticesOfSegment.Add(new List<Vector3>());
            trianglesOfSegment.Add(new List<int>());
            normalsOfSegment.Add(new List<Vector3>());
        }

        InitialSnake();
    }



    private Vector3 inputDirection = Vector3.zero; // ????
    private Vector3 snakeDirection = Vector3.zero; // ????
    
    private bool canMove = true;
    private bool turnAround = false;
    private bool straitWalk = false;
    private int segmentsOfMove = 0;

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
            snakeDirection = inputDirection;
            inputDirection = Vector3.zero;
            canMove = false;
            if (Vector3.Dot(snakeDirection, currentDirections[1]) < 0.5)
            {
                
                turnAround = true;
                Vector3 yAxis = currentDirections[1];
                Vector3 xAxis = -snakeDirection;
                Vector3 rAxis = Vector3.forward;
                Vector3 axis = Vector3.Cross(xAxis, yAxis);
                Vector3 origin = -0.5f * yAxis - 0.5f * xAxis + currentPositions[1];

                if(Vector3.Dot(axis,rAxis) > 0)
                {
                    //left turn
                    turn = 1;

                }
                else
                {
                    //right turn
                    turn = 2;
                }

             //   indexOfvertex = indexOfvertex + numberOfVerticalSlice + 1;
                coordinates[3] = origin;
                coordinates[4] = xAxis;
                coordinates[5] = yAxis;
            }
            else
            {
                for (int i = 0; i < targetPositions.Count; i++)
                {
                    targetPositions[i] = currentPositions[i] + snakeDirection;

                }

                float angle = 90;
                Vector3 yAxis, xAxis, origin;
                Vector3 rAxis = Vector3.back;
             
                Vector3 curdir = currentDirections[1];                          
                yAxis = curdir;
                xAxis = Quaternion.AngleAxis(angle, rAxis) * yAxis;
                origin = 0.5f * yAxis - 0.5f * snakeWidth * xAxis + currentPositions[1];
                coordinates[3] = origin;
                coordinates[4] = (xAxis);
                coordinates[5] = (yAxis);

                curdir = currentDirections[length - 1];
                Vector3 predir = currentDirections[length - 2];

                if (Vector3.Dot(curdir, predir) < 0.5)
                {
                    straitWalk = true;
                 
                     rAxis = Vector3.forward;
                    Vector3 axis = Vector3.Cross(curdir, predir);
                    if(Vector3.Dot(axis, rAxis) > 0)
                    {
                        shape[length - 1] = 1;
                    }
                    else
                    {
                        shape[length - 1] = 2;
                    }
                    origin = 0.5f * yAxis - 0.5f * predir + currentPositions[length-1];

                    coordinates[3 * (length - 1)] = origin;

                }
                else
                {
                    shape[length - 1] = 0;
                }
                                                               
            }
        }

        if (turnAround)
        {
            segmentsOfMove++;
            if (segmentsOfMove % speed == 0)
            {
                if (segmentsOfMove / speed <= numberOfHorizontalSlice)
                {
                    int number = segmentsOfMove / speed;
                    Mesh mesh = meshes[1];
                    Mesh headmesh = meshes[0];
                    Vector3 axis = Vector3.forward;
                    float angle = number * 90 / numberOfHorizontalSlice;

                    Vector3 origin = coordinates[3];
                    Vector3 xAxis = coordinates[4];
                    Vector3 yAxis = coordinates[5];
                    if (turn == 1)
                    {
                        indexOfvertex = indexOfvertex + numberOfVerticalSlice + 1;
                        for (int i = 1; i <= number; i++)
                        {
                            SetVerticesOfLeftTurn(verticesOfSegment[1], origin, xAxis, yAxis, i);
                        }                       
                        xAxis = Quaternion.AngleAxis(angle, axis) * xAxis;
                        yAxis = Quaternion.AngleAxis(angle, axis) * yAxis;
                        Vector3 startpoint = xAxis * (1 - snakeWidth) / 2 + origin;
                        for (int i = 1; i <= numberOfHorizontalSlice - number; i++)
                        {
                            SetVerticesOfStraitWalk(verticesOfSegment[1], startpoint, xAxis, yAxis, i);
                        }

                        SetTriangels(trianglesOfSegment[1], numberOfVerticalSlice, numberOfHorizontalSlice);

                        mesh.SetVertices(verticesOfSegment[1]);
                        mesh.SetTriangles(trianglesOfSegment[1], 0);
                        mesh.SetNormals(normalsOfSegment[1]);
                        MeshFilter mf = segments[1].GetComponent<MeshFilter>();
                        MeshRenderer mr = segments[1].GetComponent<MeshRenderer>();
                        mf.mesh = mesh;
                        mr.material = material;

                        //create head of snake
           /*             startpoint = xAxis * (1 - snakeWidth) / 2 + (1f - ((float)number / numberOfHorizontalSlice)) * yAxis + coordinates[0];
                        GenerateRectangleMesh(headVertices, headTriangles, HeadNormals, startpoint, xAxis, yAxis,
                            numberOfHorizontalSlice);
                        headmesh.SetVertices(headVertices);
                        headmesh.SetTriangles(headTriangles, 0);
                        headmesh.SetNormals(HeadNormals);
                        MeshFilter headMf = segments[0].GetComponent<MeshFilter>();
                        MeshRenderer headMr = segments[0].GetComponent<MeshRenderer>();
                        headMf.mesh = headmesh;
                        headMr.material = material;*/
                    }
                    else
                    {

                    }


                }
                else
                {
                    turnAround = false;
                    canMove = true;
                    snakeDirection = Vector3.zero;
                    segmentsOfMove = 0;
                }
            }

        }


        if (straitWalk)
        {
            segmentsOfMove++;
            if (segmentsOfMove % speed == 0)
            {
                if (segmentsOfMove / speed <= numberOfHorizontalSlice)
                {

                    for (int i = length - 2; i >= 2; i--)
                    {
                        for (int j = 0; j <= numberOfVerticalSlice; j++)
                        {

                            verticesOfSegment[i][indexOfvertex + j] = verticesOfSegment[i - 1][(indexOfvertex + j + numberOfVerticalSlice + 1)% ((numberOfHorizontalSlice + 1) * (numberOfVerticalSlice + 1))];
                        }



                    }

                    int number = segmentsOfMove / speed;                 
                    Vector3 origin = coordinates[3];
                    Vector3 xAxis = coordinates[4];
                    Vector3 yAxis = coordinates[5];

                    SetVerticesOfStraitWalk(verticesOfSegment[1], origin, xAxis, yAxis, number);
                    // updateIndexOfVertex();

                    /*            SetTriangels(trianglesOfSegment[1], numberOfVerticalSlice, numberOfHorizontalSlice);
                                Mesh mesh1 = meshes[1];
                                mesh1.SetVertices(verticesOfSegment[1]);
                                mesh1.SetTriangles(trianglesOfSegment[1], 0);
                                mesh1.SetNormals(normalsOfSegment[1]);
                                MeshFilter mf1 = segments[1].GetComponent<MeshFilter>();
                                MeshRenderer mr1 = segments[1].GetComponent<MeshRenderer>();
                                mf1.mesh = mesh1;
                                mr1.material = material;*/

                    for (int i = 1; i < length - 1; i++)
                    {
                        SetTriangels(trianglesOfSegment[i], numberOfVerticalSlice, numberOfHorizontalSlice);
                        Mesh mesh = meshes[i];
                        mesh.SetVertices(verticesOfSegment[i]);
                        mesh.SetTriangles(trianglesOfSegment[i], 0);
                        mesh.SetNormals(normalsOfSegment[i]);
                        MeshFilter mf = segments[i].GetComponent<MeshFilter>();
                        MeshRenderer mr = segments[i].GetComponent<MeshRenderer>();
                        mf.mesh = mesh;
                        mr.material = material;
                    }

                    //head
                    /* segments[0].transform.position = currentPositions[0] + number * snakeDirection * 1f / numberOfHorizontalSlice;*/

                    /*       //tail
                           if (shape[length - 1] == 2)
                           {
                               float angle = number * 90 / numberOfHorizontalSlice;
                               Vector3 point = coordinates[3 * (length - 1)];
                               segments[length - 1].transform.RotateAround(point, Vector3.back, angle * number);
                           }
       */

                }
                else
                {
                    straitWalk = false;
                    canMove = true;
                    snakeDirection = Vector3.zero;
                    segmentsOfMove = 0;
                    for (int i = 0; i < targetPositions.Count; i++)
                    {
                        currentPositions[i] =  targetPositions[i];

                    }
                }
            }

           

        }
    }


    private void InitialSnake()
    {

      //  RenderHead();
        coordinates.Add(Vector3.zero);
        coordinates.Add(Vector3.zero);
        coordinates.Add(Vector3.zero);
        shape.Add(0);

        // render body of snake
        float angle = 90;
        Vector3 yAxis, xAxis, origin;
        Vector3 rAxis = Vector3.back;
        for (int i = 1; i < currentDirections.Count - 1; i++)
        {
            Vector3 curdir = currentDirections[i];
            Vector3 nextdir = currentDirections[i + 1];
            if (Vector3.Dot(curdir, nextdir) > 0.5)
            {
                yAxis = curdir;
                xAxis = Quaternion.AngleAxis(angle, rAxis) * yAxis;
                origin = -0.5f * yAxis - 0.5f * snakeWidth * xAxis + currentPositions[i];
                shape.Add(0);
            }
            else
            {
                xAxis = -curdir;
                yAxis = nextdir;
                origin = -0.5f * yAxis - 0.5f * xAxis + currentPositions[i];
                Vector3 axis = Vector3.Cross(yAxis, xAxis);
                float dot = Vector3.Dot(axis, rAxis);
                if (dot > 0)
                {
                    shape.Add(1);
                }
                else
                {
                    shape.Add(2);
                }

            }
            coordinates.Add(origin);
            coordinates.Add(xAxis);
            coordinates.Add(yAxis);
        }

        // placeHolder
        coordinates.Add(Vector3.zero);
        coordinates.Add(Vector3.zero);
        coordinates.Add(Vector3.zero);
        shape.Add(0);

        for (int i = 1; i < shape.Count - 1; i++)
        {
            Mesh mesh = meshes[i];

            origin = coordinates[3 * i];
            xAxis = coordinates[3 * i + 1];
            yAxis = coordinates[3 * i + 2];
            if (shape[i] == 0)
            {

                GenerateRectangleMesh(verticesOfSegment[i], normalsOfSegment[i], origin, xAxis, yAxis);
            }
            else if (shape[i] == 1)
            {
                GenerateLeftBendMesh(verticesOfSegment[i], normalsOfSegment[i], origin, xAxis, yAxis);

            }
            else
            {
                GenerateRightBendMesh(verticesOfSegment[i], normalsOfSegment[i], origin, xAxis, yAxis);
            }

            AddTriangesOfSegment(trianglesOfSegment[i], numberOfVerticalSlice, numberOfHorizontalSlice);

            mesh.SetVertices(verticesOfSegment[i]);
            mesh.SetTriangles(trianglesOfSegment[i], 0);
            mesh.SetNormals(normalsOfSegment[i]);
            MeshFilter mf = segments[i].GetComponent<MeshFilter>();
            MeshRenderer mr = segments[i].GetComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = material;
        }

        RenderTail();

    }

    private void RenderHead()
    {
        // render body of snake
        float angle = 90;
        Vector3 yAxis, xAxis, origin;
        Vector3 rAxis = Vector3.back;
        yAxis = currentDirections[0];
        xAxis = Quaternion.AngleAxis(angle, rAxis) * yAxis;
        origin = -0.5f * yAxis - 0.5f * snakeWidth * xAxis + currentPositions[0];
  
        Mesh mesh = new Mesh { name = "HeadMesh" };
        GenerateRectangleMesh(verticesOfSegment[0], normalsOfSegment[0], origin, xAxis, yAxis);
        AddTriangesOfSegment(trianglesOfSegment[0], numberOfVerticalSlice, numberOfHorizontalSlice);
        mesh.SetVertices(verticesOfSegment[0]);
        mesh.SetTriangles(trianglesOfSegment[0], 0);
        mesh.SetNormals(normalsOfSegment[0]);
        MeshFilter mf = segments[0].GetComponent<MeshFilter>();
        MeshRenderer mr = segments[0].GetComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = material;

    }

    private void RenderTail()
    {
        // render body of snake
        float angle = 90;
        Vector3 yAxis, xAxis, origin;
        Vector3 rAxis = Vector3.back;
        yAxis = currentDirections[length-1];
        xAxis = Quaternion.AngleAxis(angle, rAxis) * yAxis;
        origin = -0.5f * yAxis - 0.5f * snakeWidth * xAxis + currentPositions[length-1];
    
        Mesh mesh = new Mesh { name = "HeadMesh" };
        GenerateRectangleMesh(verticesOfSegment[length - 1], normalsOfSegment[length - 1], origin, xAxis, yAxis);
        AddTriangesOfSegment(trianglesOfSegment[length - 1], numberOfVerticalSlice, numberOfHorizontalSlice);
        mesh.SetVertices(verticesOfSegment[length - 1]);
        mesh.SetTriangles(trianglesOfSegment[length - 1], 0);
        mesh.SetNormals(normalsOfSegment[length - 1]);
        MeshFilter mf = segments[length - 1].GetComponent<MeshFilter>();
        MeshRenderer mr = segments[length - 1].GetComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = material;
    }

    private void GenerateRectangleMesh(List<Vector3> vertices, List<Vector3> normals, Vector3 origin, Vector3 xaxis, Vector3 yaxis)
    {
      

        //Width of the vertical split
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        // the length of Segment is 1.0f
        float widthOfHorizontalSlice = 1.0f / numberOfHorizontalSlice;
        
        for (int i = 0; i <= numberOfHorizontalSlice; i++)
        {
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {
                Vector3 vertex = origin + i * widthOfHorizontalSlice *yaxis + widthOfVerticalSlice * j * xaxis;
                vertices.Add(vertex);
                normals.Add(Vector3.forward);
            }
        }
    }


    private void GenerateRightBendMesh(List<Vector3> vertices, List<Vector3> normals,
         Vector3 origin, Vector3 xaxis,Vector3 yaxis)
    {            
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        float angle = Mathf.PI / (2 * numberOfHorizontalSlice);
        float bendRadius =  snakeWidth / 2 + 0.5f;
        
        for (int i = 0; i <= numberOfHorizontalSlice; i++)
        {
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {
                Vector3 xcoor = (bendRadius - widthOfVerticalSlice * j) * xaxis * Mathf.Cos(i * angle);
                Vector3 ycoor = (bendRadius - widthOfVerticalSlice * j) * yaxis * Mathf.Sin(i * angle);

                Vector3 vertex = origin + xcoor + ycoor;
                vertices.Add(vertex);
                normals.Add(Vector3.forward);
            }
        }
   
    }

    private void GenerateLeftBendMesh(List<Vector3> vertices, List<Vector3> normals,
     Vector3 origin, Vector3 xaxis, Vector3 yaxis)
    {

   
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        float angle = Mathf.PI / (2  * numberOfHorizontalSlice);
        float bendRadius = (1 - snakeWidth) / 2;



        for (int i = 0; i <= numberOfHorizontalSlice; i++)
        {
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {
                Vector3 xcoor = (bendRadius + widthOfVerticalSlice * j) * xaxis * Mathf.Cos(i * angle);
                Vector3 ycoor = (bendRadius + widthOfVerticalSlice * j) * yaxis * Mathf.Sin(i * angle);

                Vector3 vertex = origin + xcoor + ycoor;
                vertices.Add(vertex);
                normals.Add(Vector3.forward);
            }
        }
    }

    private void AddTriangesOfSegment(List<int> triangles, int width , int length)
    {
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                triangles.Add(j + i * (width + 1));
                triangles.Add( j + (i + 1) * (width + 1));
                triangles.Add(j + i * (width + 1) + 1);

                triangles.Add(j + i * (width + 1) + 1);
                triangles.Add( j + (i + 1) * (width + 1));
                triangles.Add( j + (i + 1) * (width + 1) + 1);
            }
        }
    }

    private void SetVerticesOfLeftTurn(List<Vector3> vertices, Vector3 origin, Vector3 xaxis, Vector3 yaxis,int number)
    {
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        float angle = Mathf.PI / (2 * numberOfHorizontalSlice);
        float bendRadius = (1 - snakeWidth) / 2;
        
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {
                Vector3 xcoor = (bendRadius + widthOfVerticalSlice * j) * xaxis * Mathf.Cos(number * angle);
                Vector3 ycoor = (bendRadius + widthOfVerticalSlice * j) * yaxis * Mathf.Sin(number * angle);
                Vector3 vertex = origin + xcoor + ycoor;
                vertices[indexOfvertex + j] = vertex;           
           }

        updateIndexOfVertex();    
    }

    private void SetVerticesOfRightTurn(List<Vector3> vertices, Vector3 origin, Vector3 xaxis, Vector3 yaxis, int number)
    {
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        float angle = Mathf.PI / (2 * numberOfHorizontalSlice);
        float bendRadius = (1 - snakeWidth) / 2 + 0.5f;

        for (int j = 0; j <= numberOfVerticalSlice; j++)
        {
            Vector3 xcoor = (bendRadius - widthOfVerticalSlice * j) * xaxis * Mathf.Cos(number * angle);
            Vector3 ycoor = (bendRadius - widthOfVerticalSlice * j) * yaxis * Mathf.Sin(number * angle);
            Vector3 vertex = origin + xcoor + ycoor;
            vertices[indexOfvertex + j] = vertex;
        }

        updateIndexOfVertex();
    }

    private void SetVerticesOfStraitWalk(List<Vector3> vertices, Vector3 origin, Vector3 xaxis, Vector3 yaxis, int number)
    {


        //Width of the vertical split
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        // the length of Segment is 1.0f
        float widthOfHorizontalSlice = 1.0f / numberOfHorizontalSlice;
     
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {
                Vector3 vertex = origin + number * widthOfHorizontalSlice * yaxis + widthOfVerticalSlice * j * xaxis;
                vertices[indexOfvertex + j] = vertex;
            }
        updateIndexOfVertex();

    }



    private void updateIndexOfVertex()
    {
        indexOfvertex = indexOfvertex + numberOfVerticalSlice + 1;
        indexOfvertex = indexOfvertex % ((numberOfHorizontalSlice + 1) * (numberOfVerticalSlice + 1));
    }

    private void SetTriangels(List<int> triangles, int width, int length)
    {
        int total = (width + 1) * (length + 1);
        int cur = -1;
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                triangles[++cur] = (j + i * (width + 1) + indexOfvertex) % total;
                triangles[++cur] = (j + (i + 1) * (width + 1) + indexOfvertex) % total;
                triangles[++cur] = (j + i * (width + 1) + 1 + indexOfvertex) % total;
                triangles[++cur] = (j + i * (width + 1) + 1 + indexOfvertex) % total;
                triangles[++cur] = (j + (i + 1) * (width + 1) + indexOfvertex) % total;
                triangles[++cur] = (j + (i + 1) * (width + 1) + 1 + indexOfvertex) % total;           
            }
        }
    }


    private void TurnAround(Vector3 dir,int frmaeCount)
    {

    }

    Vector3 Rotate2D(Vector3 v, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        // ?Z??????z???
        float newX = v.x * cos - v.y * sin;
        float newY = v.x * sin + v.y * cos;

        return new Vector3(newX, newY, 0f);
    }

}
