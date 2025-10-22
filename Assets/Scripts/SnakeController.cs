using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.zero;
    private List<Vector3> moveDirectionsOfSegments;
    private List<Vector3> targetPositionsOfSegments;

    public List<GameObject> segments;
    private List<GameObject> bendObstruction;
    public GameObject segmentPrefab;

    public float speed = 4;

    public IEnumerator SnakeMove(Vector3 moveDirection)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Debug.Log("=== SnakeMove 开始 ===");
        // caculate movedirection of snakesegments and targetpositions of snakesegments.
        moveDirectionsOfSegments[0] = moveDirection;
        for (int i = 1; i < segments.Count; i++)
        {
            moveDirectionsOfSegments[i] = segments[i - 1].transform.position - segments[i].transform.position;
        }
        for (int i = 0; i < segments.Count; i++)
        {
            targetPositionsOfSegments[i] = segments[i].transform.position + moveDirectionsOfSegments[i];
        }

        // bendObstruction process
        for (int i = 1; i < segments.Count; i++)
        {
            if (moveDirectionsOfSegments[i] != moveDirectionsOfSegments[i - 1])
            {
                bendObstruction[i - 1].transform.position = segments[i-1].transform.position;
                bendObstruction[i - 1].SetActive(true);               
            }
        }


        // update positions of every segment
        float frameSpeed = speed / 50;
        while ((segments[0].transform.position - targetPositionsOfSegments[0]).magnitude > frameSpeed)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].transform.position = segments[i].transform.position + frameSpeed * moveDirectionsOfSegments[i];
            }
            yield return new WaitForFixedUpdate();
        }

        // snap targetpositions of segments in the end;
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].transform.position = targetPositionsOfSegments[i];
            Debug.Log($"Segment[{i}] 位置对齐完成: {segments[i].transform.position}");
        }
        Debug.Log("位置对齐完成，等待FixedUpdate");
        yield return new WaitForFixedUpdate();
        Debug.Log("FixedUpdate等待完成");
        for (int i = 0; i < bendObstruction.Count; i++)
        {
            Debug.Log($"隐藏前 bendObstruction[{i}].activeSelf: {bendObstruction[i].activeSelf}");
            if (bendObstruction[i].activeSelf == true)
            {
                bendObstruction[i].SetActive(false);
                Debug.Log($"已隐藏 bendObstruction[{i}]");
            }
        }
        Debug.Log("=== SnakeMove 结束 ===");
    }

    // Start is called before the first frame update
    void Start()
    {
        moveDirectionsOfSegments = new List<Vector3>();
        targetPositionsOfSegments = new List<Vector3>();
        for (int i = 0; i < segments.Count; i++)
        {
            moveDirectionsOfSegments.Add(Vector3.zero);
            targetPositionsOfSegments.Add(Vector3.zero);
        }
        // when still, the max number of bends is count -2.but it is count - 1 when moving.
        bendObstruction = new List<GameObject>();
        for (int i = 0; i < segments.Count - 1; i++)
        {
            GameObject obstruction = Instantiate(segmentPrefab);
            obstruction.SetActive(false);
            bendObstruction.Add(obstruction);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
