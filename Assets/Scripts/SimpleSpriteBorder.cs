using UnityEngine;

[ExecuteInEditMode]
public class SimpleSpriteBorder : MonoBehaviour
{
    [Header("Border Settings")]
    public bool showLeftBorder = true;
    public bool showRightBorder = true;
    public bool showTopBorder = true;
    public bool showBottomBorder = true;
    public Color borderColor = Color.black;
    public float borderThickness = 0.05f;

    private SpriteRenderer[] borderRenderers = new SpriteRenderer[4];

    void Start()
    {
        CreateBorders();
        UpdateBorders();
    }

    void CreateBorders()
    {
        string[] directions = { "Left", "Right", "Top", "Bottom" };

        for (int i = 0; i < 4; i++)
        {
            if (borderRenderers[i] == null)
            {
                GameObject borderObj = new GameObject($"Border_{directions[i]}");
                borderObj.transform.SetParent(transform);
                borderObj.transform.localPosition = Vector3.zero;
                borderObj.transform.localRotation = Quaternion.identity;

                borderRenderers[i] = borderObj.AddComponent<SpriteRenderer>();
                borderRenderers[i].sprite = CreateWhiteSprite();
                borderRenderers[i].color = borderColor;
                borderRenderers[i].sortingOrder = 100; // ???????
            }
        }
    }

    Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(4, 4);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                texture.SetPixel(x, y, Color.white);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
    }

    void UpdateBorders()
    {
        UpdateBorderPositions();
        UpdateBorderVisibility();
        UpdateBorderColors();
    }

    void UpdateBorderPositions()
    {
        if (borderRenderers[0] != null) // Left
        {
            borderRenderers[0].transform.localPosition = new Vector3(-0.5f - borderThickness / 2, 0, 0);
            borderRenderers[0].transform.localScale = new Vector3(borderThickness, 1, 1);
        }

        if (borderRenderers[1] != null) // Right
        {
            borderRenderers[1].transform.localPosition = new Vector3(0.5f + borderThickness / 2, 0, 0);
            borderRenderers[1].transform.localScale = new Vector3(borderThickness, 1, 1);
        }

        if (borderRenderers[2] != null) // Top
        {
            borderRenderers[2].transform.localPosition = new Vector3(0, 0.5f + borderThickness / 2, 0);
            borderRenderers[2].transform.localScale = new Vector3(1, borderThickness, 1);
        }

        if (borderRenderers[3] != null) // Bottom
        {
            borderRenderers[3].transform.localPosition = new Vector3(0, -0.5f - borderThickness / 2, 0);
            borderRenderers[3].transform.localScale = new Vector3(1, borderThickness, 1);
        }
    }

    void UpdateBorderVisibility()
    {
        if (borderRenderers[0] != null) borderRenderers[0].gameObject.SetActive(showLeftBorder);
        if (borderRenderers[1] != null) borderRenderers[1].gameObject.SetActive(showRightBorder);
        if (borderRenderers[2] != null) borderRenderers[2].gameObject.SetActive(showTopBorder);
        if (borderRenderers[3] != null) borderRenderers[3].gameObject.SetActive(showBottomBorder);
    }

    void UpdateBorderColors()
    {
        foreach (var renderer in borderRenderers)
        {
            if (renderer != null) renderer.color = borderColor;
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UpdateBorders();
        }
    }

    [ContextMenu("Refresh Borders")]
    public void RefreshBorders()
    {
        UpdateBorders();
    }
}