using UnityEngine;

public class GridLines : MonoBehaviour
{
    [SerializeField]
    public int rows = 10;  // ??
    [SerializeField]
    public int columns = 10;  // ??
    [SerializeField]
    public float cellSize = 1f;  // ????????
    [SerializeField]
    public Color lineColor = Color.yellow;  // ??????
    [SerializeField]
    public float lineAlpha = 0.5f;  // ???????
    private Vector3 gridOrigin;

    void Start()
    {
        gridOrigin = transform.position;
        DrawGrid();
    }

    void DrawGrid()
    {
        for (int i = 0; i <= rows; i++)
        {
            DrawLine(gridOrigin + new Vector3(0, i * cellSize, 0), gridOrigin + new Vector3(columns * cellSize, i * cellSize, 0));
        }
        for (int j = 0; j <= columns; j++)
        {
            DrawLine(gridOrigin + new Vector3(j * cellSize, 0, 0), gridOrigin + new Vector3(j * cellSize, rows * cellSize, 0));
        }
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        // ????????
        Color colorWithAlpha = new Color(lineColor.r, lineColor.g, lineColor.b, lineAlpha);
        lr.startColor = colorWithAlpha;
        lr.endColor = colorWithAlpha;

        lr.sortingLayerName = "UI";   // ??????? Sorting Layer
        lr.sortingOrder = 10;         // ?????

        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.SetPosition(0, start + new Vector3(0, 0, -0.1f));
        lr.SetPosition(1, end + new Vector3(0, 0, -0.1f));
        line.transform.parent = this.transform;  // ?????? GridLinesObject ????
    }
}