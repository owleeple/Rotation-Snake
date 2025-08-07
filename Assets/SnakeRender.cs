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
    private List<Vector3> targetPositions;
    public List<Vector3> currentPositions;
    private List<Vector3> currentDirections;
    private List<GameObject> segments;

    private List<Vector3> coordinates = new List<Vector3>();
    private List<int> shape = new List<int>();
    private int turn = 0;
    // private LinkedList<Vector3> forwardDirections;


    // Start is called before the first frame update
    void Start()
    {

        segments = new List<GameObject>();

        foreach (Transform child in transform)
        {
            segments.Add(child.gameObject);
        }

        int length = segments.Count;    
        targetPositions = new List<Vector3>(length); 
        
        currentDirections = new List<Vector3>(length + 2);
        Vector3 headdir = currentPositions[0]- currentPositions[1];
        currentDirections.Add(headdir) ;
        currentDirections.Add(headdir) ;
        for (int i = 2; i < length + 1; i++)
        {    
            currentDirections.Add(currentPositions[i - 2] - currentPositions[i-1]);         
        }

        Vector3 taildir = currentPositions[length - 2] - currentPositions[length - 1];
        currentDirections.Add(taildir);

        InitialSnake();
    }



    private Vector3 inputDirection = Vector3.zero; // ????
    private Vector3 snakeDirection = Vector3.zero; // ????
    
    private bool canMove = true;
    private bool turnAround = false;
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
                turn = 1;
                turnAround = true;
                Vector3 yAxis = currentDirections[1];
                Vector3 xAxis = -snakeDirection;
                Vector3 rAxis = Vector3.forward;
                Vector3 axis = Vector3.Cross(xAxis, yAxis);
                Vector3 origin = -0.5f * yAxis - 0.5f * xAxis + currentPositions[1];
                coordinates[0] = origin;
                coordinates[1] = xAxis;
                coordinates[2] = yAxis;
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

                for (int i = 1; i < currentDirections.Count - 1; i++)
                {
                    Vector3 curdir = currentDirections[i];
                    Vector3 nextdir = currentDirections[i + 1];
                    if (Vector3.Dot(curdir, nextdir) > 0.5)
                    {
                        yAxis = curdir;
                        xAxis = Quaternion.AngleAxis(angle, rAxis) * yAxis;
                        origin = -0.5f * yAxis - 0.5f * snakeWidth * xAxis + currentPositions[i - 1];
                        shape.Add(0);
                    }
                    else
                    {
                        xAxis = -curdir;
                        yAxis = nextdir;
                        origin = -0.5f * yAxis - 0.5f * xAxis + currentPositions[i - 1];
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
                    Mesh mesh = new Mesh { name = "SegmentMesh" };
                    List<Vector3> vertices = new List<Vector3>();
                    List<int> triangles = new List<int>();
                    List<Vector3> normals = new List<Vector3>();

                    Mesh headmesh = new Mesh { name = "SegmentHead" };
                    List<Vector3> headVertices = new List<Vector3>();
                    List<int> headTriangles = new List<int>();
                    List<Vector3> HeadNormals = new List<Vector3>();

                    Vector3 axis = Vector3.forward;
                    float angle = number * 90 / numberOfHorizontalSlice;

                    Vector3 origin = coordinates[3];
                    Vector3 xAxis = coordinates[4];
                    Vector3 yAxis = coordinates[5];
                    if (turn == 1)
                    {                     
                        GenerateLeftBendMesh(vertices, triangles, normals, origin, xAxis, yAxis,
                            0, number * bendSegments);
                        xAxis = Quaternion.AngleAxis(angle, axis) * xAxis;
                        yAxis = Quaternion.AngleAxis(angle, axis) * yAxis;
                        Vector3 startpoint =  xAxis * (1 - snakeWidth) / 2 + origin;
                     

                        GenerateRectangleMesh(vertices, triangles, normals, startpoint, xAxis, yAxis,
                            numberOfHorizontalSlice - number);

                        mesh.SetVertices(vertices);
                        mesh.SetTriangles(triangles, 0);
                        mesh.SetNormals(normals);
                        MeshFilter mf = segments[1].GetComponent<MeshFilter>();
                        MeshRenderer mr = segments[1].GetComponent<MeshRenderer>();
                        mf.mesh = mesh;
                        mr.material = material;

                        //create head of snake
                        startpoint = xAxis * (1 - snakeWidth) / 2 + (1f -((float)number / numberOfHorizontalSlice)) * yAxis + coordinates[0];
                        GenerateRectangleMesh(headVertices, headTriangles, HeadNormals, startpoint, xAxis, yAxis,
                            numberOfHorizontalSlice);
                        headmesh.SetVertices(headVertices);
                        headmesh.SetTriangles(headTriangles, 0);
                        headmesh.SetNormals(HeadNormals);
                        MeshFilter headMf = segments[0].GetComponent<MeshFilter>();
                        MeshRenderer headMr = segments[0].GetComponent<MeshRenderer>();
                        headMf.mesh = headmesh;
                        headMr.material = material;
                    }
                    else
                    {

                    }


                }else
                {
                    turnAround = false;
                    canMove = true;
                    snakeDirection = Vector3.zero;
                    segmentsOfMove = 0;
                }
            }
           
        }
    }


    private void InitialSnake()
    {
       
        float angle = 90;
        Vector3 yAxis, xAxis,origin;
        Vector3 rAxis = Vector3.back;

        for (int i = 1; i < currentDirections.Count - 1; i++)
        {
            Vector3 curdir = currentDirections[i];
            Vector3 nextdir = currentDirections[i + 1];
            if(Vector3.Dot(curdir,nextdir) > 0.5)
            {
                yAxis = curdir;
                xAxis = Quaternion.AngleAxis(angle, rAxis) * yAxis;
                origin = -0.5f * yAxis - 0.5f * snakeWidth * xAxis + currentPositions[i-1];            
                shape.Add(0);
            }
            else
            {
                xAxis = -curdir;
                yAxis = nextdir;
                origin = -0.5f * yAxis - 0.5f * xAxis + currentPositions[i-1];
                Vector3 axis = Vector3.Cross(yAxis, xAxis);
                float dot = Vector3.Dot(axis, rAxis);
                if(dot > 0)
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


        for (int i = 0; i < shape.Count; i++)
        {
            Mesh mesh = new Mesh { name = "SegmentMesh" };
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            origin = coordinates[3 * i];
            xAxis = coordinates[3 * i + 1];
            yAxis = coordinates[3 * i + 2];
            if (shape[i] == 0)
            {
             
                GenerateRectangleMesh(vertices, triangles, normals, origin, xAxis, yAxis, numberOfHorizontalSlice);
            }
            else if (shape[i] == 1)
            {
               
                GenerateLeftBendMesh(vertices, triangles, normals, origin, xAxis, yAxis, 0,numberOfHorizontalSlice * bendSegments);
            }else
            {
                GenerateRightBendMesh(vertices, triangles, normals, origin, xAxis, yAxis, 0, numberOfHorizontalSlice * bendSegments);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            MeshFilter mf = segments[i].GetComponent<MeshFilter>();
            MeshRenderer mr = segments[i].GetComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = material;

        }

       

        

        /*        Mesh mesh = new Mesh { name = "SegmentMesh" };
                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Vector3> normals = new List<Vector3>();
                //List<Vector2> uvs = new List<Vector2>();

                Vector3 curdir = currentDirections[0];
                Vector3 yAxis = curdir.normalized;
                Vector3 axis = Vector3.forward;   
                float angle = -90f;               
                Vector3 xAxis = (Quaternion.AngleAxis(angle, axis) * yAxis).normalized;
                Vector3 curpos = currentPositions[0];
                Vector3 startpoint = curpos - 0.5f * snakeWidth * xAxis - 0.5f * yAxis;
                GenerateRectangleMesh(vertices, triangles, normals, startpoint, xAxis, yAxis, 5);
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.SetNormals(normals);

                GameObject head = segments[0];
                MeshFilter mf = head.GetComponent<MeshFilter>();
                MeshRenderer mr = head.GetComponent<MeshRenderer>();
                mf.mesh = mesh;
                mr.material = material;*/


    }

    private void CactulateCurrentDirections(LinkedList<Vector3> initialPos, LinkedList<Vector3> curtDir)
    {
        throw new NotImplementedException();
    }

    private void AddVertexAndNormols(LinkedList<Vector3> bodypositions, List<Vector3> Vertexes)
    {

    }

    private Mesh GenerateSegmentMesh(Vector3 previous, Vector3 current, Vector3 next,int FrameCount)
    {
        Mesh mesh = new Mesh { name = "SegmentMesh" };
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

      //  int numberOfMove = 5;
        Vector3 origin, xAxis, yAxis;
        origin = new Vector3(0, 0, 0);
        xAxis = new Vector3(1, 0, 0);
        yAxis = new Vector3(0, 1, 0);
        Vector3 axis = Vector3.forward;
        float angle = 45f;
        xAxis = Quaternion.AngleAxis(angle, axis) * xAxis;
        yAxis = Quaternion.AngleAxis(angle, axis) * yAxis;





       // GenerateRectangleMesh(vertices, triangles, normals, origin, xAxis, yAxis, numberOfMove);

      //  GenerateBendMesh(vertices, triangles, normals, origin, xAxis, yAxis, numberOfMove);

        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles,0);
        mesh.SetNormals(normals);
        return mesh;
       
    }

    private void GenerateRectangleMesh(List<Vector3> vertices, List<int> triangles, List<Vector3> normals,
        Vector3 origin, Vector3 xaxis, Vector3 yaxis, int numberOfSegments)
    {
        // get the offset of vertices
        int offset = vertices.Count;

        //Width of the vertical split
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        // the length of Segment is 1.0f
        float widthOfHorizontalSlice = 1.0f / numberOfHorizontalSlice;
        float bendRadius = (1 - snakeWidth) / 2;
        for (int i = 0; i <= numberOfSegments; i++)
        {
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {
                Vector3 vertex = origin + i * widthOfHorizontalSlice *yaxis + widthOfVerticalSlice * j * xaxis;
                vertices.Add(vertex);
                normals.Add(Vector3.forward);
            }
        }

 
        for (int i = 0; i < numberOfSegments; i++)
        {
            for (int j = 0; j < numberOfVerticalSlice; j++)
            {
                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1) + 1);

                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1) + 1);
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1) + 1);
            }
        }

    
    }


    private void GenerateRightBendMesh(List<Vector3> vertices, List<int> triangles, List<Vector3> normals,
         Vector3 origin, Vector3 xaxis,Vector3 yaxis, int numberOfStart, int numberOfEnd)
    {
        
        // get the offset of vertices
        int offset = vertices.Count;
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        float angle = Mathf.PI / (2 * bendSegments * numberOfHorizontalSlice);
        float bendRadius =  snakeWidth / 2 + 0.5f;
        
        for (int i = numberOfStart; i <= numberOfEnd; i++)
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

        float number = numberOfEnd - numberOfStart;
        for (int i = 0; i < number; i++)
        {
            for (int j = 0; j < numberOfVerticalSlice; j++)
            {
                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1) + 1);

                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1) + 1);
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1) + 1);
            }
        }
    }

    private void GenerateLeftBendMesh(List<Vector3> vertices, List<int> triangles, List<Vector3> normals,
     Vector3 origin, Vector3 xaxis, Vector3 yaxis, int numberOfStart, int numberOfEnd)
    {

        // get the offset of vertices
        int offset = vertices.Count;
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        float angle = Mathf.PI / (2 * bendSegments * numberOfHorizontalSlice);
        float bendRadius = (1 - snakeWidth) / 2;



        for (int i = numberOfStart; i <= numberOfEnd; i++)
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

        float number = numberOfEnd - numberOfStart;
        for (int i = 0; i < number; i++)
        {
            for (int j = 0; j < numberOfVerticalSlice; j++)
            {
                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1) + 1);

                triangles.Add(offset + j + i * (numberOfVerticalSlice + 1) + 1);
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1));
                triangles.Add(offset + j + (i + 1) * (numberOfVerticalSlice + 1) + 1);
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
