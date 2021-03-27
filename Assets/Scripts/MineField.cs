using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class MineField : MonoBehaviour
{
    public Cell[,] mineFieldCells;

    [SerializeField] private int rowsNum = 16;
    [SerializeField] private int colsNum = 30;
    [SerializeField] private int minesNum = 99;    
    [SerializeField] private GameObject cell = null;
    [SerializeField] private Sprite cellClosed = null;
    [SerializeField] private Sprite cellFlagged = null;
    [SerializeField] private Sprite cellQuestioned = null;
    [SerializeField] private Sprite cellExploded = null;
    [SerializeField] private Sprite[] cellOpened = new Sprite[9];

    [SerializeField] private Difficult[] difficultLevels;

    [SerializeField] private UnityEvent onStartGame;
    [SerializeField] private UnityEvent onGameBegin;
    [SerializeField] private UnityEvent onAllMinesLocated;
    [SerializeField] private UnityEvent onGameLost;
    [SerializeField] private UnityEvent<int> onChangeMinesLeft;

    private int currentDifficult;
    private int minesLeft = 0;
    private bool isGameOver = false;
    private int openedCells = 0;       

    private GridLayoutGroup gridLayoutGroup = null;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        mineFieldCells = new Cell[rowsNum, colsNum];
        gridLayoutGroup = GetComponent<GridLayoutGroup>();      
    }

    void Start()
    {
        if (PlayerPrefs.HasKey("DifficultLevel"))
        {
            currentDifficult = PlayerPrefs.GetInt("DifficultLevel");
        }
            
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = colsNum;
        StartNewGame();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F2))
        {
            StartNewGame();
        }
    }

    public void StartNewGame()
    {
        ApplyDifficult();
        gridLayoutGroup.constraintCount = colsNum;
        GenerateMineField();
        isGameOver = false;
        openedCells = 0;        
        onStartGame.Invoke();
    }

    private void ApplyDifficult()
    {
        try
        {
            rowsNum = difficultLevels[currentDifficult].rowsNum;
            colsNum = difficultLevels[currentDifficult].colsNum;
            minesNum = difficultLevels[currentDifficult].minesNum;
        }
        catch (Exception)
        {

        }
    }

    public void ReDrawCell(CellUI cellUI, bool isFlagged)
    {
        if (isFlagged)
        {
            cellUI.GetComponent<Image>().sprite = cellFlagged;
        }
        else
        {
            cellUI.GetComponent<Image>().sprite = cellClosed;
        }
    }

    public void ReDrawCell(CellUI cellUI, int returnCode)
    {
        if (returnCode == -1)
        {
            cellUI.GetComponent<Image>().sprite = cellExploded;
        }
        else
        {
            cellUI.GetComponent<Image>().sprite = cellOpened[returnCode];
        }
    }

    public void DrawMineField()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < rowsNum; i++)
        {
            for (int k = 0; k < colsNum; k++)
            {
                GameObject newCell = Instantiate(cell, transform);

                CellUI cellUI = newCell.GetComponent<CellUI>();
                cellUI.SetCell(mineFieldCells[i, k]);
                cellUI.SetRowPos(i);
                cellUI.SetColPos(k);

                if (mineFieldCells[i, k].IsOpened)
                {
                    if (mineFieldCells[i, k].HasMine)
                    {
                        newCell.GetComponent<Image>().sprite = cellExploded;
                    }
                    else
                    {
                        int numberOfBombsAround = mineFieldCells[i, k].MinesAround;
                        newCell.GetComponent<Image>().sprite = cellOpened[numberOfBombsAround];
                    }
                }
                else
                {
                    if (mineFieldCells[i, k].IsFlagged)
                    {
                        newCell.GetComponent<Image>().sprite = cellFlagged;
                    }
                    else
                    {
                        newCell.GetComponent<Image>().sprite = cellClosed;
                    }                    
                }
            }
        }
    }

    internal void OpenCellsAround(int rowPos, int colPos, bool openFlagged)
    {
        for (int i = rowPos - 1; i <= rowPos + 1; i++)
        {
            if (i < 0 || i >= rowsNum) { }
            else
            {
                for (int k = colPos - 1; k <= colPos + 1; k++)
                {
                    if (k < 0 || k >= colsNum) { }
                    else
                    {
                        if (mineFieldCells[i, k] != null && !mineFieldCells[i, k].IsOpened)
                        {
                            if (openFlagged)
                            {
                                ProceedOpenCell(openFlagged, i, k);
                            }
                            else
                            {                        
                                if (!mineFieldCells[i, k].IsFlagged)
                                {
                                    ProceedOpenCell(openFlagged, i, k);
                                }
                            }
                        }
                    }
                }
            }
        }       
    }

    private void ProceedOpenCell(bool openFlagged, int i, int k)
    {
        int returnCode = mineFieldCells[i, k].OpenCell();
        AddOpenedCell();

        if (returnCode == 0)
        {
            OpenCellsAround(i, k, openFlagged);
        }
    }

    internal void ProceedOpenCellsAroundExceptFlagged(int rowPos, int colPos)
    {
        if (CalculateFlaggedCellsAround(rowPos, colPos) == mineFieldCells[rowPos, colPos].MinesAround)
        {
            OpenCellsAround(rowPos, colPos, false);
        }
    }

    private int CalculateFlaggedCellsAround(int row, int col)
    {
        int counter = 0;

        for (int i = row - 1; i <= row + 1; i++)
        {
            if (i < 0 || i >= rowsNum) { }
            else
            {
                for (int k = col - 1; k <= col + 1; k++)
                {
                    if (k < 0 || k >= colsNum) { }
                    else
                    {
                        if (mineFieldCells[i, k] != null && mineFieldCells[i, k].IsFlagged)
                        {
                            counter++;
                        }
                    }
                }
            }

        }

        return counter;
    }

    private void GenerateMineField()
    {
        ClearMineFieldCellsArray();

        int tempMinesCounter = minesNum;

        while (tempMinesCounter > 0)
        {
            int randomRow = UnityEngine.Random.Range(0, rowsNum);
            int randomCol = UnityEngine.Random.Range(0, colsNum);

            if (mineFieldCells[randomRow, randomCol] == null)
            {
                mineFieldCells[randomRow, randomCol] = new Cell(true);
                tempMinesCounter--;
            }
        }

        AssignCellWithNoBomb();

        DrawMineField();

        minesLeft = minesNum;

        onChangeMinesLeft.Invoke(minesLeft);
    }

    private void ClearMineFieldCellsArray()
    {
        for (int i = 0; i < rowsNum; i++)
        {
            for (int k = 0; k < colsNum; k++)
            {
                mineFieldCells[i, k] = null;
            }
        }
    }

    private void AssignCellWithNoBomb()
    {
        for (int i = 0; i < rowsNum; i++)
        {
            for (int k = 0; k < colsNum; k++)
            {                
                if (mineFieldCells[i, k] == null)
                {
                    int bombsAround = CalculateNumberOfBombsAroundCell(i, k);
                    mineFieldCells[i, k] = new Cell(bombsAround);
                }
            }
        }
    }

    private int CalculateNumberOfBombsAroundCell(int row, int col)
    {
        int counter = 0;

        for (int i = row - 1; i <= row + 1; i++)
        {
            if (i < 0 || i >= rowsNum) { }
            else
            {
                for (int k = col - 1; k <= col + 1; k++)
                {
                    if (k < 0 || k >= colsNum) { }
                    else {
                        if (mineFieldCells[i, k] != null && mineFieldCells[i, k].HasMine)
                        {
                            counter++;
                        }
                    }
                }
            }
            
        }

        return counter;
    }

    public void GameLost()
    {
        isGameOver = true;
        onGameLost.Invoke();
    }

    public void AddOpenedCell()
    {
        openedCells++;

        bool ifAllMinesLocated = ((rowsNum * colsNum) - openedCells) == minesNum;

        if (ifAllMinesLocated)
        {
            isGameOver = true;
            onAllMinesLocated.Invoke();
            onChangeMinesLeft.Invoke(minesLeft);
        }
    }

    public void ChangeMinesLeft(int value)
    {
        minesLeft += value;
        onChangeMinesLeft.Invoke(minesLeft);
    }

    public void GameBegin()
    {
        onGameBegin.Invoke();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetDifficult(int difficultValue)
    {
        currentDifficult = Mathf.Clamp(difficultValue, 0, difficultLevels.Length);
        PlayerPrefs.SetInt("DifficultLevel", currentDifficult);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    public class Difficult
    {
        public int rowsNum = 9;
        public int colsNum = 9;
        public int minesNum = 10;
    }
}