using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwallowRendering : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 position;
    private GameObject g;
  // private bool isActive = false;
    public float snakeWidth = 0.8f;
    // Vertical segmentation
    public int numberOfVerticalSlice = 5;
    public int numberOfHorizontalSlice = 10;
    public float widthOfHorizontalSlice;
    public float widthOfVerticalSlice;
    public Material material;

    void Start()
    {
        
        g = GameObject.Find("GeometricObject");
        List<Vector3> positions = new List<Vector3>();
       // int i = 0;
        foreach (Transform child in g.transform)
        {
            Debug.Log(child.gameObject.name);
            Debug.Log("index : " + child.GetSiblingIndex());
            positions.Add(child.position);
        }
        widthOfHorizontalSlice = 1f / numberOfHorizontalSlice;
        widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;

        RenderSwallowBody(positions);
    }

    private void RenderSwallowBody(List<Vector3> positions)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        List<Vector3> directions = new List<Vector3>();
        directions[0] = positions[1] - positions[0];
        for (int i = 1; i < positions.Count; i++)
        {
            directions[i] = directions[i] - directions[i - 1];
        }

        Vector3 yAxis = Vector3.zero;
        Vector3 xAxis = Vector3.zero;
        Vector3 origin = Vector3.zero;
        Vector3 axis = Vector3.forward;
        // render first segment
        yAxis = directions[0];
        xAxis = Quaternion.AngleAxis(-90, axis) * yAxis;
        origin = positions[0] - 0.5f * yAxis - 0.5f * (snakeWidth) * xAxis;
        for (int i = 0; i <= numberOfVerticalSlice; i++)
        {
            Vector3 vertex = origin + widthOfVerticalSlice * i * xAxis;
            vertices.Add(vertex);
            normals.Add(Vector3.forward);
        }

        GenerateRectangleMesh(vertices, normals, origin, xAxis, yAxis);

        for (int i = 1; i < positions.Count; i++)
        {

        }


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
          //  isActive = true;
        }
    }


    private void GenerateRectangleMesh(List<Vector3> vertices, List<Vector3> normals, Vector3 origin, Vector3 xaxis, Vector3 yaxis)
    {

        //Width of the vertical split
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        // the length of Segment is 1.0f
        float widthOfHorizontalSlice = 1.0f / numberOfHorizontalSlice;

        for (int i = 1; i <= numberOfHorizontalSlice; i++)
        {
            for (int j = 0; j <= numberOfVerticalSlice; j++)
            {
                Vector3 vertex = origin + i * widthOfHorizontalSlice * yaxis + widthOfVerticalSlice * j * xaxis;
                vertices.Add(vertex);
                normals.Add(Vector3.forward);
            }
        }
    }

    private void GenerateRightBendMesh(List<Vector3> vertices, List<Vector3> normals,
        Vector3 origin, Vector3 xaxis, Vector3 yaxis)
    {
        float widthOfVerticalSlice = snakeWidth / numberOfVerticalSlice;
        float angle = Mathf.PI / (2 * numberOfHorizontalSlice);
        float bendRadius = snakeWidth / 2 + 0.5f;

        for (int i = 1; i <= numberOfHorizontalSlice; i++)
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
        float angle = Mathf.PI / (2 * numberOfHorizontalSlice);
        float bendRadius = (1 - snakeWidth) / 2;



        for (int i = 1; i <= numberOfHorizontalSlice; i++)
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
}
