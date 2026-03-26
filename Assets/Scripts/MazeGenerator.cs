/* ----------------------------
 * 2D Maze Generator for Unity.
 * Uses Deapth-First searching 
 * and Recursive Backtracking.
 * ----------------------------
 * Generates a 2x2 centre room in 
 * the middle of the Maze.
 * ----------------------------
 * Author: c00pala
 * ~13/05/2018~
 * ---------------------------- 
 * Edited by: Joshua Doyle 22474715
 * Removed a lot of dead ends and added a training version to allow the Opponent to learn
 */
   

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze generation values:")]
    [Tooltip("How many cells tall is the maze. MUST be an even number. Minimum value of 4.")]
    public int mazeRows;

    [Tooltip("How many cells wide is the maze. MUST be an even number. Minimum value of 4.")]
    public int mazeColumns;

    [Header("Maze object variables:")]
    [SerializeField] private GameObject cellPrefab;
    public bool disableCellSprite;

    [Header("Simple Training Layout")]
    [SerializeField] private bool useSimpleTrainingLayout = true;
    [SerializeField] private SimpleLayoutType simpleLayoutType = SimpleLayoutType.Vertical;
    [SerializeField] private int numberOfLines = 3;

    private int centreSize = 2;

    private Dictionary<Vector2, Cell> allCells = new Dictionary<Vector2, Cell>();
    private List<Cell> unvisited = new List<Cell>();
    private List<Cell> stack = new List<Cell>();

    private Cell[] centreCells = new Cell[4];

    private Cell currentCell;
    private Cell checkCell;

    private Vector2[] neighbourPositions = new Vector2[]
    {
        new Vector2(-1, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(0, -1)
    };

    private float cellSize;
    private GameObject mazeParent;

    public enum SimpleLayoutType
    {
        Vertical,
        Horizontal
    }

    private void Start()
    {
        GenerateMaze(mazeRows, mazeColumns);
    }

    private void GenerateMaze(int rows, int columns)
    {
        if (mazeParent != null)
            DeleteMaze();

        mazeRows = rows;
        mazeColumns = columns;
        CreateLayout();
    }

    private void CreateExtraOpenings(int openingCount)
    {
        List<Cell> cells = new List<Cell>(allCells.Values);

        int created = 0;
        int attempts = 0;
        int maxAttempts = openingCount * 20;

        while (created < openingCount && attempts < maxAttempts)
        {
            attempts++;

            Cell cell = cells[Random.Range(0, cells.Count)];
            int wallID = Random.Range(1, 5);

            Vector2 neighborPos = cell.gridPos;

            if (wallID == 1) neighborPos += Vector2.left;
            else if (wallID == 2) neighborPos += Vector2.right;
            else if (wallID == 3) neighborPos += Vector2.up;
            else if (wallID == 4) neighborPos += Vector2.down;

            if (!allCells.ContainsKey(neighborPos))
                continue;

            Cell neighbor = allCells[neighborPos];

            // Open both touching walls
            RemoveWall(cell.cScript, wallID);

            if (wallID == 1) RemoveWall(neighbor.cScript, 2);
            else if (wallID == 2) RemoveWall(neighbor.cScript, 1);
            else if (wallID == 3) RemoveWall(neighbor.cScript, 4);
            else if (wallID == 4) RemoveWall(neighbor.cScript, 3);

            created++;
        }
    }

    public void CreateLayout()
    {
        InitValues();

        Vector2 startPos = new Vector2(
            -(cellSize * (mazeColumns / 2)) + (cellSize / 2),
            -(cellSize * (mazeRows / 2)) + (cellSize / 2)
        );

        Vector2 spawnPos = startPos;

        for (int x = 1; x <= mazeColumns; x++)
        {
            for (int y = 1; y <= mazeRows; y++)
            {
                GenerateCell(spawnPos, new Vector2(x, y));
                spawnPos.y += cellSize;
            }

            spawnPos.y = startPos.y;
            spawnPos.x += cellSize;
        }

        if (useSimpleTrainingLayout)
        {
            BuildSimpleTrainingLayout();
        }
        else
        {
            CreateCentre();
            RunAlgorithm();
            CreateExtraOpenings(6);
        }
    }

    public void RunAlgorithm()
    {
        unvisited.Remove(currentCell);

        while (unvisited.Count > 0)
        {
            List<Cell> unvisitedNeighbours = GetUnvisitedNeighbours(currentCell);

            if (unvisitedNeighbours.Count > 0)
            {
                checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                stack.Add(currentCell);
                CompareWalls(currentCell, checkCell);
                currentCell = checkCell;
                unvisited.Remove(currentCell);
            }
            else if (stack.Count > 0)
            {
                currentCell = stack[stack.Count - 1];
                stack.Remove(currentCell);
            }
        }
    }

    private void BuildSimpleTrainingLayout()
    {
        OpenAllInternalWalls();

        if (simpleLayoutType == SimpleLayoutType.Vertical)
            BuildVerticalLines();
        else
            BuildHorizontalLines();

        Debug.Log("Simple training layout generated.");
    }

    private void OpenAllInternalWalls()
    {
        for (int x = 1; x <= mazeColumns; x++)
        {
            for (int y = 1; y <= mazeRows; y++)
            {
                Cell cell = allCells[new Vector2(x, y)];

                if (x < mazeColumns)
                {
                    Cell rightCell = allCells[new Vector2(x + 1, y)];
                    SetWall(cell.cScript, 2, false);
                    SetWall(rightCell.cScript, 1, false);
                }

                if (y < mazeRows)
                {
                    Cell upCell = allCells[new Vector2(x, y + 1)];
                    SetWall(cell.cScript, 3, false);
                    SetWall(upCell.cScript, 4, false);
                }
            }
        }
    }

    private void BuildVerticalLines()
    {
        List<int> lineBoundaries = GetEvenlySpacedBoundaries(mazeColumns, numberOfLines);

        foreach (int boundaryX in lineBoundaries)
        {
            if (boundaryX < 1 || boundaryX >= mazeColumns)
                continue;

            for (int y = 2; y <= mazeRows - 1; y++)
            {
                Cell leftCell = allCells[new Vector2(boundaryX, y)];
                Cell rightCell = allCells[new Vector2(boundaryX + 1, y)];

                SetWall(leftCell.cScript, 2, true);
                SetWall(rightCell.cScript, 1, true);
            }
        }
    }

    private void BuildHorizontalLines()
    {
        List<int> lineBoundaries = GetEvenlySpacedBoundaries(mazeRows, numberOfLines);

        foreach (int boundaryY in lineBoundaries)
        {
            if (boundaryY < 1 || boundaryY >= mazeRows)
                continue;

            for (int x = 2; x <= mazeColumns - 1; x++)
            {
                Cell bottomCell = allCells[new Vector2(x, boundaryY)];
                Cell topCell = allCells[new Vector2(x, boundaryY + 1)];

                SetWall(bottomCell.cScript, 3, true);
                SetWall(topCell.cScript, 4, true);
            }
        }
    }

    private List<int> GetEvenlySpacedBoundaries(int count, int lineCount)
    {
        List<int> boundaries = new List<int>();

        if (lineCount <= 0)
            return boundaries;

        for (int i = 1; i <= lineCount; i++)
        {
            int boundary = Mathf.RoundToInt((float)i * count / (lineCount + 1));

            boundary = Mathf.Clamp(boundary, 1, count - 1);

            if (!boundaries.Contains(boundary))
                boundaries.Add(boundary);
        }

        return boundaries;
    }

    public void MakeExit()
    {
        List<Cell> edgeCells = new List<Cell>();

        foreach (KeyValuePair<Vector2, Cell> cell in allCells)
        {
            if (cell.Key.x == 0 || cell.Key.x == mazeColumns || cell.Key.y == 0 || cell.Key.y == mazeRows)
            {
                edgeCells.Add(cell.Value);
            }
        }

        Cell newCell = edgeCells[Random.Range(0, edgeCells.Count)];

        if (newCell.gridPos.x == 0) RemoveWall(newCell.cScript, 1);
        else if (newCell.gridPos.x == mazeColumns) RemoveWall(newCell.cScript, 2);
        else if (newCell.gridPos.y == mazeRows) RemoveWall(newCell.cScript, 3);
        else RemoveWall(newCell.cScript, 4);

        Debug.Log("Maze generation finished.");
    }

    public List<Cell> GetUnvisitedNeighbours(Cell curCell)
    {
        List<Cell> neighbours = new List<Cell>();
        Cell nCell = curCell;
        Vector2 cPos = curCell.gridPos;

        foreach (Vector2 p in neighbourPositions)
        {
            Vector2 nPos = cPos + p;

            if (allCells.ContainsKey(nPos))
                nCell = allCells[nPos];

            if (unvisited.Contains(nCell))
                neighbours.Add(nCell);
        }

        return neighbours;
    }

    public void CompareWalls(Cell cCell, Cell nCell)
    {
        if (nCell.gridPos.x < cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 2);
            RemoveWall(cCell.cScript, 1);
        }
        else if (nCell.gridPos.x > cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 1);
            RemoveWall(cCell.cScript, 2);
        }
        else if (nCell.gridPos.y > cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 4);
            RemoveWall(cCell.cScript, 3);
        }
        else if (nCell.gridPos.y < cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 3);
            RemoveWall(cCell.cScript, 4);
        }
    }

    public void RemoveWall(CellScript cScript, int wallID)
    {
        SetWall(cScript, wallID, false);
    }

    public void SetWall(CellScript cScript, int wallID, bool state)
    {
        if (wallID == 1) cScript.wallL.SetActive(state);
        else if (wallID == 2) cScript.wallR.SetActive(state);
        else if (wallID == 3) cScript.wallU.SetActive(state);
        else if (wallID == 4) cScript.wallD.SetActive(state);
    }

    public void CreateCentre()
    {
        centreCells[0] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2) + 1)];
        RemoveWall(centreCells[0].cScript, 4);
        RemoveWall(centreCells[0].cScript, 2);

        centreCells[1] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2) + 1)];
        RemoveWall(centreCells[1].cScript, 4);
        RemoveWall(centreCells[1].cScript, 1);

        centreCells[2] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2))];
        RemoveWall(centreCells[2].cScript, 3);
        RemoveWall(centreCells[2].cScript, 2);

        centreCells[3] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2))];
        RemoveWall(centreCells[3].cScript, 3);
        RemoveWall(centreCells[3].cScript, 1);

        List<int> rndList = new List<int> { 0, 1, 2, 3 };
        int startCell = rndList[Random.Range(0, rndList.Count)];
        rndList.Remove(startCell);
        currentCell = centreCells[startCell];

        foreach (int c in rndList)
        {
            unvisited.Remove(centreCells[c]);
        }
    }

    public void GenerateCell(Vector2 pos, Vector2 keyPos)
    {
        Cell newCell = new Cell();

        newCell.gridPos = keyPos;
        newCell.cellObject = Instantiate(cellPrefab, pos, cellPrefab.transform.rotation);

        if (mazeParent != null)
            newCell.cellObject.transform.parent = mazeParent.transform;

        newCell.cellObject.name = "Cell - X:" + keyPos.x + " Y:" + keyPos.y;
        newCell.cScript = newCell.cellObject.GetComponent<CellScript>();

        if (disableCellSprite)
            newCell.cellObject.GetComponent<SpriteRenderer>().enabled = false;

        foreach (Transform child in newCell.cellObject.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Walls");
        }

        allCells[keyPos] = newCell;
        unvisited.Add(newCell);
    }

    public void DeleteMaze()
    {
        if (mazeParent != null)
            Destroy(mazeParent);
    }

    public void InitValues()
    {
        allCells.Clear();
        unvisited.Clear();
        stack.Clear();

        if (IsOdd(mazeRows)) mazeRows--;
        if (IsOdd(mazeColumns)) mazeColumns--;

        if (mazeRows <= 3) mazeRows = 4;
        if (mazeColumns <= 3) mazeColumns = 4;

        cellSize = cellPrefab.transform.localScale.x;

        mazeParent = new GameObject();
        mazeParent.transform.position = Vector2.zero;
        mazeParent.name = "Maze";
    }

    public bool IsOdd(int value)
    {
        return value % 2 != 0;
    }

    public class Cell
    {
        public Vector2 gridPos;
        public GameObject cellObject;
        public CellScript cScript;
    }
}








// Tinkered with code from github to change the maze to my liking
// Opponent struggled with too many corridors and dead ends so made maze something simple like 3 lines with an outer border to stop opponent from leaving maze.
// Made training mode which includes a simpler layout and kept old code for actualy gameplay.


