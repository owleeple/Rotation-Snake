using UnityEngine;

/// <summary>
/// Attach to your Square GameObject (the one that has the mesh for the tile).
/// Creates 4 child quads for borders and keeps their thickness stable.
/// Mode: useScreenPixels = true -> borderWidthPixels (screen px) converted to world units via camera
///       useScreenPixels = false -> borderWidthWorld (world units)
/// </summary>
[ExecuteAlways]
public class AddBorderChildren : MonoBehaviour
{
    public Camera targetCamera; // set to your main camera or leave null to Camera.main
    public bool useScreenPixels = false;
    public float borderWidthPixels = 2f; // used if useScreenPixels==true
    public float borderWidthWorld = 0.05f; // used if useScreenPixels==false

    // child names
    private readonly string[] names = { "Border_Left", "Border_Right", "Border_Top", "Border_Bottom" };
    private Transform[] borders = new Transform[4];

    void OnEnable()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        EnsureChildren();
        UpdateBorders();
    }

    void OnValidate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        EnsureChildren();
        UpdateBorders();
    }

    void Update()
    {
        // If you want to update only in editor move, keep Update. For runtime, keep Update as well.
        UpdateBorders();
    }

    void EnsureChildren()
    {
        for (int i = 0; i < 4; i++)
        {
            if (borders[i] == null)
            {
                var t = transform.Find(names[i]);
                if (t == null)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    go.name = names[i];
                    go.transform.SetParent(transform, false);
                    // Remove collider if created
                    var collider = go.GetComponent<Collider>();
                    if (collider) DestroyImmediate(collider);
                    // Use unlit color material (replace with your border material)
                    var rend = go.GetComponent<MeshRenderer>();
                    rend.sharedMaterial = new Material(Shader.Find("Unlit/Color")) { color = Color.black };
                    borders[i] = go.transform;
                }
                else borders[i] = t;
            }
        }
    }

    void UpdateBorders()
    {
        // Compute tile bounds in local space (assuming default quad [-0.5,0.5])
        // If your square mesh has different size, adapt these values.
        Vector2 half = new Vector2(0.5f, 0.5f);
        // Compute world thickness
        float thicknessWorld = borderWidthWorld;
        if (useScreenPixels)
        {
            if (targetCamera == null) targetCamera = Camera.main;
            // convert borderWidthPixels -> world units at object's position (orthographic assumed)
            if (targetCamera.orthographic)
            {
                // world height = cam.orthographicSize * 2
                float worldHeight = targetCamera.orthographicSize * 2f;
                float screenHeight = Screen.height;
                thicknessWorld = (borderWidthPixels / screenHeight) * worldHeight;
            }
            else
            {
                // perspective: approximate at object's Z distance
                float dist = Mathf.Abs(targetCamera.transform.position.z - transform.position.z);
                float frustumHeight = 2.0f * dist * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                thicknessWorld = (borderWidthPixels / Screen.height) * frustumHeight;
            }
        }

        // Set transforms for 4 quads
        // Left
        SetBorderTransform(borders[0],
            localPos: new Vector3(-half.x + thicknessWorld * 0.5f, 0f, 0f),
            localScale: new Vector3(thicknessWorld, half.y * 2f + thicknessWorld * 0f, 1f));
        // Right
        SetBorderTransform(borders[1],
            localPos: new Vector3(half.x - thicknessWorld * 0.5f, 0f, 0f),
            localScale: new Vector3(thicknessWorld, half.y * 2f + thicknessWorld * 0f, 1f));
        // Top
        SetBorderTransform(borders[2],
            localPos: new Vector3(0f, half.y - thicknessWorld * 0.5f, 0f),
            localScale: new Vector3(half.x * 2f + thicknessWorld * 0f, thicknessWorld, 1f));
        // Bottom
        SetBorderTransform(borders[3],
            localPos: new Vector3(0f, -half.y + thicknessWorld * 0.5f, 0f),
            localScale: new Vector3(half.x * 2f + thicknessWorld * 0f, thicknessWorld, 1f));
    }

    void SetBorderTransform(Transform t, Vector3 localPos, Vector3 localScale)
    {
        if (t == null) return;
        t.localPosition = localPos;
        t.localScale = localScale;
        t.localRotation = Quaternion.identity;
    }
}
