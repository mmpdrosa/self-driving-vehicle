using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pathfinding : MonoBehaviour
{
    [SerializeField] private bool displaySearchTree = false;

    [SerializeField] private bool displayGrid = false;

    private AStartGrid grid;

    [SerializeField] private Car startCar, endCar;

    HybridAStar hybridAStar;

    public void Find()
    {
        grid.Run();
        hybridAStar.FindPath(grid, startCar, endCar);
    }


    void Start()
    {
        grid = GetComponent<AStartGrid>();
        hybridAStar = GetComponent<HybridAStar>();
    }

    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            return;
        }

        if (hybridAStar == null)
        {
            return;
        }

        if (displayGrid)
        {
            grid.DisplayGrid();
        }

        if (displaySearchTree)
        {
            hybridAStar.DisplaySearchTree();
        }
    }

    private void DrawSearchTree()
    {
    }

    private void DrawPath()
    {
    }
}