using UnityEngine;

[ExecuteInEditMode]  // ??????
public class BorderController : MonoBehaviour
{
    [Header("Border Settings")]
    public bool showLeftBorder = true;
    public bool showRightBorder = true;
    public bool showTopBorder = true;
    public bool showBottomBorder = true;

    [Header("Border Appearance")]
    public Color borderColor = Color.black;
    [Range(0, 0.5f)]
    public float borderWidth = 0.05f;

    [Header("Other Settings")]
    public bool useSplitColor = true;
    public bool useBorder = true;

    private Renderer renderer;
    private MaterialPropertyBlock propertyBlock;

    void OnEnable()
    {
        Initialize();
        UpdateMaterialProperties();
    }

    void Initialize()
    {
        if (renderer == null)
            renderer = GetComponent<Renderer>();

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }

    void UpdateMaterialProperties()
    {
        Initialize();

        if (renderer == null) return;

        renderer.GetPropertyBlock(propertyBlock);

        // ????????
        propertyBlock.SetFloat("_ShowLeft", showLeftBorder ? 1 : 0);
        propertyBlock.SetFloat("_ShowRight", showRightBorder ? 1 : 0);
        propertyBlock.SetFloat("_ShowTop", showTopBorder ? 1 : 0);
        propertyBlock.SetFloat("_ShowBottom", showBottomBorder ? 1 : 0);

        // ??????
        propertyBlock.SetColor("_BorderColor", borderColor);
        propertyBlock.SetFloat("_BorderWidth", borderWidth);
        propertyBlock.SetFloat("_UseSplitColor", useSplitColor ? 1 : 0);
        propertyBlock.SetFloat("_UseBorder", useBorder ? 1 : 0);

        renderer.SetPropertyBlock(propertyBlock);
    }

    // ?Inspector????????????????????
    void OnValidate()
    {
        // ????????????
        if (!Application.isPlaying)
        {
            UpdateMaterialProperties();
        }
    }

    void Update()
    {
        // ????????????????
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateMaterialProperties();
        }
#endif
    }

    // ???????????
    public void SetBorderVisibility(bool left, bool right, bool top, bool bottom)
    {
        showLeftBorder = left;
        showRightBorder = right;
        showTopBorder = top;
        showBottomBorder = bottom;
        UpdateMaterialProperties();
    }

    public void SetBorderColor(Color color)
    {
        borderColor = color;
        UpdateMaterialProperties();
    }

    public void SetBorderWidth(float width)
    {
        borderWidth = Mathf.Clamp(width, 0, 0.5f);
        UpdateMaterialProperties();
    }

    // ???????????
    [ContextMenu("Refresh Border Settings")]
    public void RefreshBorderSettings()
    {
        UpdateMaterialProperties();
    }
}