using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int width;
    public int height;
    public Vector2 size = new (40f, 40f);
    public float cellSize = 0.5f;

    public Cell[,] data;

    void Awake()
    {
        width = Mathf.RoundToInt(size.x / cellSize);
        height = Mathf.RoundToInt(size.y / cellSize);

        data = new Cell[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 position = new Vector3(
                    i * cellSize - size.x / 2 + cellSize / 2,
                    0,
                    j * cellSize - size.y / 2 + cellSize / 2
                );

                data[i, j] = new Cell(position);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3 cellCenter = new Vector3(
                    i * cellSize - size.x / 2 + cellSize / 2,
                    0,
                    j * cellSize - size.y / 2 + cellSize / 2
                );

                Gizmos.DrawWireCube(transform.TransformPoint(cellCenter), new Vector3(cellSize, 0, cellSize));
            }
        }
    }

    public IntVector2 GetCellFromWorldPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        
        int x = Mathf.FloorToInt((localPosition.x + size.x / 2) / cellSize);
        int y = Mathf.FloorToInt((localPosition.z + size.y / 2) / cellSize);

        // x = Mathf.Clamp(x, 0, width - 1);
        // y = Mathf.Clamp(y, 0, height - 1);

        return new IntVector2(x, y);
    }

    public bool IsCellValid(IntVector2 cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
        {
            return false;
        }

        return true;
    }
}
