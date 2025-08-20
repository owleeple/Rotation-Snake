using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SnakeRender : MonoBehaviour
{
    public float snakeWidth = 0.8f;
    // Vertical segmentation
    public int numberOfVerticalSlice = 5;
    public int numberOfHorizontalSlice = 10;
    public Material material;
    // private float bendRadius = 0.5f;
   // private int bendSegments = 12;
    public int speed = 2;
    public int count = 2;
    private int length;
    private int indexOfvertex = 0;
    // set up coordinates for vertice of right order
    private List<Vector3> coordinates = new List<Vector3>();
    private List<int> shape = new List<int>();
   // private int turn = 0;
    // create list of vertices for new algorithm that only update head and tail in list
    private List<List<Vector3>> verticesOfSegment = new List<List<Vector3>>();
    private List<List<Vector2>> uvsOfSegment = new List<List<Vector2>>();
    private List<List<int>> trianglesOfSegment = new List<List<int>>();
    private List<List<Vector3>> normalsOfSegment = new List<List<Vector3>>();

    public List<GameObject> segments;
    private List<Vector3> targetPositions;
    public List<Vector3> currentPositions;
    public List<Vector3> currentDirections;
    public List<Mesh> meshes;
    private float turnAngle;
    public Vector3[] backupVerticesOfhead;
  
    // Start is called before the first frame update
    void Start()
    {
        

        turnAngle = 90 / numberOfHorizontalSlice;

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

        SetPositionOfColliderOfSegments();


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

    public void SetPositionOfColliderOfSegments()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            BoxCollider2D col2d = segments[i].GetComponent<BoxCollider2D>();
            col2d.offset = currentPositions[i];
        }
    }

    private Vector3 inputDirection = Vector3.zero; // ????
    public Vector3 snakeDirection = Vector3.zero; // ????
    
    public void MoveTheWholeSnake(Vector3 dir, float distance)
    {
        Vector3 translation = dir.normalized * distance;   
        Matrix4x4 moveMatrix = Matrix4x4.Translate(translation);
        for (int i = 0; i < meshes.Count; i++)
        {
            Mesh mesh = meshes[i];
            Vector3[] vertices = mesh.vertices;
            for (int j = 0; j < vertices.Length; j++)
            {
                Vector3 pos = vertices[j];
                Vector3 newPos = moveMatrix.MultiplyPoint3x4(pos);
                vertices[j] = newPos;
                verticesOfSegment[i][j] = newPos;
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }


    public void UpdateCurrentPositionAndCurrentDirection()
    {
        for (int i = 0; i < targetPositions.Count; i++)
        {
            currentPositions[i] = targetPositions[i];

        }

        for (int i = 1; i < currentPositions.Count; i++)
        {
            currentDirections[i] = currentPositions[i - 1] - currentPositions[i];
        }

        currentDirections[0] = currentPositions[0] - currentPositions[1];
    }

    public void UpdateSegmentsInMiddleForStraitWalk(int number)
    {
        // update body of others from privious segment.
        for (int i = length - 2; i >= 2; i--)
        {
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {

                verticesOfSegment[i][indexOfvertex + j] = verticesOfSegment[i - 1][(indexOfvertex + j + numberOfVerticalSlice + 1) % ((numberOfHorizontalSlice + 1) * (numberOfVerticalSlice + 1))];
            }
        }


        // fetch data of second segment
        Vector3 origin = coordinates[3];
        Vector3 xAxis = coordinates[4];
        Vector3 yAxis = coordinates[5];

        SetVerticesOfStraitWalk(verticesOfSegment[1], origin, xAxis, yAxis, number);


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
    }

    public void UpdateHeadForStraitWalk()
    {
        //head strait walk
        Mesh headmesh = meshes[0];
        Vector3[] headvertices = headmesh.vertices;
        for (int i = 0; i < headvertices.Length; i++)
        {
            headvertices[i] += snakeDirection / numberOfHorizontalSlice;
        }
        headmesh.vertices = headvertices;
        headmesh.RecalculateBounds();
        headmesh.RecalculateNormals();
    }

    public void UpdateTail()
    {
        //tail
        Mesh tailmesh = meshes[length - 1];
        Vector3[] tailvertices = tailmesh.vertices;

        if (shape[length - 1] == 0)
        {
            for (int i = 0; i < tailvertices.Length; i++)
            {
                tailvertices[i] += currentDirections[length - 1] / numberOfHorizontalSlice;
            }
        }
        else
        {

            Vector3 pivot = coordinates[3 * (length - 1)];

            for (int i = 0; i < tailvertices.Length; i++)
            {
                Vector3 relative = tailvertices[i] - pivot;
                if (shape[length - 1] == 1)
                {
                    relative = Quaternion.AngleAxis(turnAngle, Vector3.forward) * relative;
                }
                else
                {
                    relative = Quaternion.AngleAxis(turnAngle, Vector3.back) * relative;
                }

                tailvertices[i] = relative + pivot;
            }

        }

        tailmesh.vertices = tailvertices;
        tailmesh.RecalculateBounds();
        tailmesh.RecalculateNormals();
    }

    public void UpdateHeadForTurnAround(int number)
    {
        // fetch data of second segment
        Vector3 origin = coordinates[3];
        Vector3 xAxis = coordinates[4];
        Vector3 yAxis = coordinates[5];

        Vector3 newHeadPosition = Vector3.zero;
        Vector3 axis = Vector3.forward;
        float angle;
        if (shape[1] == 1)
        {
            angle = number * 90 / numberOfHorizontalSlice;
        }
        else
        {
            angle = -number * 90 / numberOfHorizontalSlice;
        }

        if (shape[1] == 1)
        {
            xAxis = Quaternion.AngleAxis(angle, axis) * xAxis;
            yAxis = Quaternion.AngleAxis(angle, axis) * yAxis;
            newHeadPosition = 0.5f * xAxis + origin + 0.5f * yAxis;
        }
        else
        {
            xAxis = Quaternion.AngleAxis(angle, axis) * xAxis;
            yAxis = Quaternion.AngleAxis(angle, axis) * yAxis;
            newHeadPosition = 0.5f * xAxis + origin + 0.5f * yAxis;
        }



        // render head
        // translate
        Vector3 translateVector = newHeadPosition - currentPositions[0];
        Matrix4x4 T = Matrix4x4.Translate(translateVector);
        Matrix4x4 Tneg = Matrix4x4.Translate(-newHeadPosition);
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        Matrix4x4 R = Matrix4x4.Rotate(rotation);
        Matrix4x4 Tpos = Matrix4x4.Translate(newHeadPosition);


        Matrix4x4 M = Tpos * R * Tneg * T;
        Mesh headmesh = meshes[0];
        Vector3[] headvertices = new Vector3[backupVerticesOfhead.Length];
        //List<Vector3> rotateVertices = new List<Vector3>();
        for (int i = 0; i < headvertices.Length; i++)
        {
            headvertices[i] = M.MultiplyPoint3x4(backupVerticesOfhead[i]);
        }
        headmesh.vertices = headvertices;
        headmesh.RecalculateBounds();
        headmesh.RecalculateNormals();
    }

    public void UpdateSegmentsInMiddleForTurnAround(int number)
    {
        // update body of others from privious segment.
        for (int i = length - 2; i >= 2; i--)
        {
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {

                verticesOfSegment[i][indexOfvertex + j] = verticesOfSegment[i - 1][(indexOfvertex + j + numberOfVerticalSlice + 1) % ((numberOfHorizontalSlice + 1) * (numberOfVerticalSlice + 1))];
            }

        }


        // fetch data of second segment
        Vector3 origin = coordinates[3];
        Vector3 xAxis = coordinates[4];
        Vector3 yAxis = coordinates[5];

        // update second segment

        if (shape[1] == 1)
        {
            SetVerticesOfLeftTurn(verticesOfSegment[1], origin, xAxis, yAxis, number);
        }
        else
        {
            SetVerticesOfRightTurn(verticesOfSegment[1], origin, xAxis, yAxis, number);
        }

        // rerender segments except tail and head
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
    }

    public void SetDataOfSecondSegmentForStraitWalk()
    {
        float angle = 90;
        Vector3 yAxis, xAxis, origin;
        Vector3 rAxis = Vector3.back;

        //1. process second segment
        yAxis = currentDirections[1];
        xAxis = Quaternion.AngleAxis(angle, rAxis) * yAxis;
        origin = 0.5f * yAxis - 0.5f * snakeWidth * xAxis + currentPositions[1];
        coordinates[3] = origin;
        coordinates[4] = (xAxis);
        coordinates[5] = (yAxis);
    }

    public void SetDataOfTail()
    {
        // process tail
        Vector3 rAxis, axis, origin;
        Vector3 curdir = currentDirections[length - 1];
        Vector3 predir = currentDirections[length - 2];

        if (Vector3.Dot(curdir, predir) < 0.5)
        {
            rAxis = Vector3.forward;
            axis = Vector3.Cross(curdir, predir);
            if (Vector3.Dot(axis, rAxis) > 0)
            {
                // left turn
                shape[length - 1] = 1;
            }
            else
            {
                // right turn
                shape[length - 1] = 2;
            }
            origin = 0.5f * curdir + 0.5f * predir + currentPositions[length - 1];

            coordinates[3 * (length - 1)] = origin;

        }
        else
        {
            shape[length - 1] = 0;
        }
    }

    public void SetDataOfSecondSegmentForTurnAround()
    {
        Vector3 yAxis = currentDirections[1];
        Vector3 xAxis = -snakeDirection;
        Vector3 rAxis = Vector3.forward;
        Vector3 axis = Vector3.Cross(xAxis, yAxis);
        Vector3 origin = -0.5f * yAxis - 0.5f * xAxis + currentPositions[0];
        coordinates[3] = origin;
        coordinates[4] = xAxis;
        coordinates[5] = yAxis;

        if (Vector3.Dot(axis, rAxis) > 0)
        {
            //left turn
            shape[1] = 1;

        }
        else
        {
            //right turn
            shape[1] = 2;
        }
    }

    public void UpdateTargetPosition()
    {
        for (int i = 1; i < targetPositions.Count; i++)
        {
            targetPositions[i] = currentPositions[i - 1];

        }

        targetPositions[0] = currentPositions[0] + snakeDirection;
    }

    public void UpdateTargetPositionWhenMoveWhole()
    {
        for (int i = 0; i < targetPositions.Count; i++)
        {
            targetPositions[i] = currentPositions[i] + snakeDirection;
        }  
    }

    private void InitialSnake()
    {

         RenderHead();
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
  
        Mesh mesh = meshes[0];
        GenerateRectangleMesh(verticesOfSegment[0], normalsOfSegment[0], origin, xAxis, yAxis);
        AddTriangesOfSegment(trianglesOfSegment[0], numberOfVerticalSlice, numberOfHorizontalSlice - 1);
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
    
        Mesh mesh = meshes[length-1];
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
                vertices[(indexOfvertex + j) % ((numberOfHorizontalSlice + 1) * (numberOfVerticalSlice + 1))] = vertex;           
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
            if(indexOfvertex + j > 35)
            {
                Debug.Log(indexOfvertex);
                Debug.Log(j);
            }
            vertices[(indexOfvertex + j)%((numberOfHorizontalSlice + 1) * (numberOfVerticalSlice + 1))] = vertex;
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
                vertices[(indexOfvertex + j) % ((numberOfHorizontalSlice + 1) * (numberOfVerticalSlice + 1))] = vertex;
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

   

}
