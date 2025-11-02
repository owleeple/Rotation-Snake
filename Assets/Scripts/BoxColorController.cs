using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BoxColorController : MonoBehaviour
{
    private Material mat;
    private int horizontalRotation = 1;
    private int verticalRotation = 1;

    void Awake()
    {
        // ??????????????
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        mat = new Material(sr.material);
        sr.material = mat;
       // SetRatio(0.7f);
       // SetDirection(1);

    }

    /// <summary>
    /// roasted ratio : 0 ~ 1
    /// </summary>
    public void SetRatio(float ratio)
    {
        mat.SetFloat("_Ratio", Mathf.Clamp01(ratio));
    }

    /// <summary>
    /// adjust roasted direction by horizontal rotation
    /// </summary>
    public void ReverseHorizontalRotation()
    {
        horizontalRotation = -horizontalRotation;
    }

    /// <summary>
    /// adjust roasted direction by vertical rotation
    /// </summary>
    public void ReverseVerticalRotation()
    {
        verticalRotation = -verticalRotation;
    }

    /// <summary>
    /// ?????? (0=Right, 1=Left, 2=Up, 3=Down)
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        dir = dir.normalized;

        int directionIndex = 0;

        if (Vector2.Dot(dir * horizontalRotation, Vector2.right) > 0.9f) 
            directionIndex = 0;
        else if (Vector2.Dot(dir * horizontalRotation, Vector2.left) > 0.9f) 
            directionIndex = 1;
        else if (Vector2.Dot(dir * verticalRotation, Vector2.up) > 0.9f) 
            directionIndex = 2;
        else if (Vector2.Dot(dir * verticalRotation, Vector2.down) > 0.9f) 
            directionIndex = 3;

        mat.SetInt("_Direction", directionIndex);
    }

    /// <summary>
    /// ??????
    /// </summary>
    public void SetColors(Color colorA, Color colorB)
    {
        mat.SetColor("_ColorA", colorA);
        mat.SetColor("_ColorB", colorB);
    }
}
