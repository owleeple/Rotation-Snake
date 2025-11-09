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

    private List<GameObject> segments;
    private List<GameObject> bendObstruction;
    public GameObject segmentPrefab;

    private float speed = 4;

    public IEnumerator SnakeMove(Vector3 moveDirection)
    {
        Console.OutputEncoding = Encoding.UTF8;

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

        }

     //   yield return new WaitForFixedUpdate();

        for (int i = 0; i < bendObstruction.Count; i++)
        {

            if (bendObstruction[i].activeSelf == true)
            {
                bendObstruction[i].SetActive(false);

            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        segments = new List<GameObject>();
        foreach (Transform child in transform)
        {
            segments.Add(child.gameObject);
        }
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
            SpriteRenderer renderer = obstruction.GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 1;
            obstruction.SetActive(false);
            bendObstruction.Add(obstruction);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSpeed(float speedvalue)
    {
        speed = speedvalue;
    }

    public GameObject GetHead()
    {
        return segments[0];
    }
}
