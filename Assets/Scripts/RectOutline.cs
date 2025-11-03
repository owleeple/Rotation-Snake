using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RectOutline : MonoBehaviour
{
    public Camera targetCamera;
    public bool useScreenPixels = false;
    public float borderWidthPixels = 2f;
    public float borderWidthWorld = 0.05f;
    public Color borderColor = Color.black;

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 5; // looped rectangle
        lr.loop = false;
        lr.useWorldSpace = false; // will set local points
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = borderColor;
        lr.numCapVertices = 4; // rounded caps
    }

    void Update()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        float thickness = borderWidthWorld;
        if (useScreenPixels && targetCamera != null)
        {
            if (targetCamera.orthographic)
            {
                float worldHeight = targetCamera.orthographicSize * 2f;
                thickness = (borderWidthPixels / Screen.height) * worldHeight;
            }
            else
            {
                float dist = Mathf.Abs(targetCamera.transform.position.z - transform.position.z);
                float frustumHeight = 2.0f * dist * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                thickness = (borderWidthPixels / Screen.height) * frustumHeight;
            }
        }

        lr.startWidth = lr.endWidth = thickness;

        // Assuming quad with local size 1x1 centered at 0
        float hx = 0.5f;
        float hy = 0.5f;
        lr.SetPosition(0, new Vector3(-hx, -hy, 0f));
        lr.SetPosition(1, new Vector3(-hx, hy, 0f));
        lr.SetPosition(2, new Vector3(hx, hy, 0f));
        lr.SetPosition(3, new Vector3(hx, -hy, 0f));
        lr.SetPosition(4, new Vector3(-hx, -hy, 0f));
    }
}
