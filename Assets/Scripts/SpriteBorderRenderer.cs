using UnityEngine;

[ExecuteInEditMode]
public class SpriteBorderRenderer : MonoBehaviour
{
    [Header("Border Settings")]
    public bool showLeftBorder = true;
    public bool showRightBorder = true;
    public bool showTopBorder = true;
    public bool showBottomBorder = true;
    public bool showTopLeftCorner = true;
    public bool showTopRightCorner = true;
    public bool showBottomLeftCorner = true;
    public bool showBottomRightCorner = true;

    public Color borderColor = Color.black;
    [Range(0.01f, 0.5f)]
    public float borderWidth = 0.05f;

    [Header("Advanced Settings")]
    public bool useCustomSprite = false;
    public Sprite borderSprite; // ??????sprite

    // ????
    private GameObject leftBorder;
    private GameObject rightBorder;
    private GameObject topBorder;
    private GameObject bottomBorder;

    // ?????
    private GameObject topLeftCorner;
    private GameObject topRightCorner;
    private GameObject bottomLeftCorner;
    private GameObject bottomRightCorner;

    private Sprite defaultSprite;

    void Start()
    {
        CreateDefaultSpriteIfNeeded();
        CreateBorderObjects();
        UpdateBorder();
    }

    void CreateDefaultSpriteIfNeeded()
    {
        if (!useCustomSprite && defaultSprite == null)
        {
            // ??4x4??????????
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                colors[i] = Color.white;
            }
            texture.SetPixels(colors);
            texture.Apply();

            defaultSprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100);
            defaultSprite.name = "DefaultBorderSprite";
        }
    }

    Sprite GetBorderSprite()
    {
        return useCustomSprite ? borderSprite : defaultSprite;
    }

    void CreateBorderObjects()
    {
        // ????GameObject
        leftBorder = CreateBorderObject("LeftBorder");
        rightBorder = CreateBorderObject("RightBorder");
        topBorder = CreateBorderObject("TopBorder");
        bottomBorder = CreateBorderObject("BottomBorder");

        // ?????GameObject
        topLeftCorner = CreateBorderObject("TopLeftCorner");
        topRightCorner = CreateBorderObject("TopRightCorner");
        bottomLeftCorner = CreateBorderObject("BottomLeftCorner");
        bottomRightCorner = CreateBorderObject("BottomRightCorner");

        // ??????
        leftBorder.transform.SetParent(transform);
        rightBorder.transform.SetParent(transform);
        topBorder.transform.SetParent(transform);
        bottomBorder.transform.SetParent(transform);
        topLeftCorner.transform.SetParent(transform);
        topRightCorner.transform.SetParent(transform);
        bottomLeftCorner.transform.SetParent(transform);
        bottomRightCorner.transform.SetParent(transform);

        // ?????????
        leftBorder.transform.localPosition = Vector3.zero;
        rightBorder.transform.localPosition = Vector3.zero;
        topBorder.transform.localPosition = Vector3.zero;
        bottomBorder.transform.localPosition = Vector3.zero;
        topLeftCorner.transform.localPosition = Vector3.zero;
        topRightCorner.transform.localPosition = Vector3.zero;
        bottomLeftCorner.transform.localPosition = Vector3.zero;
        bottomRightCorner.transform.localPosition = Vector3.zero;

        leftBorder.transform.localRotation = Quaternion.identity;
        rightBorder.transform.localRotation = Quaternion.identity;
        topBorder.transform.localRotation = Quaternion.identity;
        bottomBorder.transform.localRotation = Quaternion.identity;
        topLeftCorner.transform.localRotation = Quaternion.identity;
        topRightCorner.transform.localRotation = Quaternion.identity;
        bottomLeftCorner.transform.localRotation = Quaternion.identity;
        bottomRightCorner.transform.localRotation = Quaternion.identity;
    }

    GameObject CreateBorderObject(string name)
    {
        GameObject obj = new GameObject(name);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = GetBorderSprite();
        sr.color = borderColor;

        // ????????????
        sr.sortingOrder = 1;

        return obj;
    }

    void UpdateBorder()
    {
        UpdateEdgePositions();
        UpdateCornerPositions();
        UpdateVisibility();
        UpdateColors();
    }

    void UpdateEdgePositions()
    {
        // ??
        if (leftBorder != null)
        {
            leftBorder.transform.localPosition = new Vector3(-0.5f + borderWidth / 2, 0, -0.01f);
            leftBorder.transform.localScale = new Vector3(borderWidth, 1 - borderWidth * 2, 1);
        }

        // ??
        if (rightBorder != null)
        {
            rightBorder.transform.localPosition = new Vector3(0.5f - borderWidth / 2, 0, -0.01f);
            rightBorder.transform.localScale = new Vector3(borderWidth, 1 - borderWidth * 2, 1);
        }

        // ??
        if (topBorder != null)
        {
            topBorder.transform.localPosition = new Vector3(0, 0.5f - borderWidth / 2, -0.01f);
            topBorder.transform.localScale = new Vector3(1 - borderWidth * 2, borderWidth, 1);
        }

        // ??
        if (bottomBorder != null)
        {
            bottomBorder.transform.localPosition = new Vector3(0, -0.5f + borderWidth / 2, -0.01f);
            bottomBorder.transform.localScale = new Vector3(1 - borderWidth * 2, borderWidth, 1);
        }
    }

    void UpdateCornerPositions()
    {
        // ???
        if (topLeftCorner != null)
        {
            topLeftCorner.transform.localPosition = new Vector3(-0.5f + borderWidth / 2, 0.5f - borderWidth / 2, -0.01f);
            topLeftCorner.transform.localScale = new Vector3(borderWidth, borderWidth, 1);
        }

        // ???
        if (topRightCorner != null)
        {
            topRightCorner.transform.localPosition = new Vector3(0.5f - borderWidth / 2, 0.5f - borderWidth / 2, -0.01f);
            topRightCorner.transform.localScale = new Vector3(borderWidth, borderWidth, 1);
        }

        // ???
        if (bottomLeftCorner != null)
        {
            bottomLeftCorner.transform.localPosition = new Vector3(-0.5f + borderWidth / 2, -0.5f + borderWidth / 2, -0.01f);
            bottomLeftCorner.transform.localScale = new Vector3(borderWidth, borderWidth, 1);
        }

        // ???
        if (bottomRightCorner != null)
        {
            bottomRightCorner.transform.localPosition = new Vector3(0.5f - borderWidth / 2, -0.5f + borderWidth / 2, -0.01f);
            bottomRightCorner.transform.localScale = new Vector3(borderWidth, borderWidth, 1);
        }
    }

    void UpdateVisibility()
    {
        SetActiveSafe(leftBorder, showLeftBorder);
        SetActiveSafe(rightBorder, showRightBorder);
        SetActiveSafe(topBorder, showTopBorder);
        SetActiveSafe(bottomBorder, showBottomBorder);
        SetActiveSafe(topLeftCorner, showTopLeftCorner);
        SetActiveSafe(topRightCorner, showTopRightCorner);
        SetActiveSafe(bottomLeftCorner, showBottomLeftCorner);
        SetActiveSafe(bottomRightCorner, showBottomRightCorner);
    }

    void UpdateColors()
    {
        UpdateSpriteRendererColor(leftBorder, borderColor);
        UpdateSpriteRendererColor(rightBorder, borderColor);
        UpdateSpriteRendererColor(topBorder, borderColor);
        UpdateSpriteRendererColor(bottomBorder, borderColor);
        UpdateSpriteRendererColor(topLeftCorner, borderColor);
        UpdateSpriteRendererColor(topRightCorner, borderColor);
        UpdateSpriteRendererColor(bottomLeftCorner, borderColor);
        UpdateSpriteRendererColor(bottomRightCorner, borderColor);
    }

    void UpdateSpriteRendererColor(GameObject obj, Color color)
    {
        if (obj != null)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }
    }

    void SetActiveSafe(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            CreateDefaultSpriteIfNeeded();
            if (leftBorder != null) // ???????
            {
                UpdateBorder();
            }
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && leftBorder != null)
        {
            UpdateBorder();
        }
#endif
    }

    [ContextMenu("Force Refresh Border")]
    public void ForceRefreshBorder()
    {
        CreateDefaultSpriteIfNeeded();
        UpdateBorder();
    }

    // ????????...
    public void SetEdgeVisibility(bool left, bool right, bool top, bool bottom)
    {
        showLeftBorder = left;
        showRightBorder = right;
        showTopBorder = top;
        showBottomBorder = bottom;
        UpdateBorder();
    }

    public void SetCornerVisibility(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
    {
        showTopLeftCorner = topLeft;
        showTopRightCorner = topRight;
        showBottomLeftCorner = bottomLeft;
        showBottomRightCorner = bottomRight;
        UpdateBorder();
    }

    public void SetBorderColor(Color color)
    {
        borderColor = color;
        UpdateBorder();
    }
}