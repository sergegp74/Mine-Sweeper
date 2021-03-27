using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellUI : MonoBehaviour, IPointerClickHandler
{
    public Cell cell = null;

    public static bool gameStarted = false;

    private int rowPos = 0;
    private int colPos = 0;    
    private MineField mineField = null;

    private void Awake()
    {
        mineField = FindObjectOfType<MineField>();        
    }

    public void SetCell(Cell cell)
    {
        this.cell = cell;
    }

    public void SetRowPos(int num)
    {
        rowPos = num;
    }

    public void SetColPos(int num)
    {
        colPos = num;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (mineField.IsGameOver)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            if (cell.IsOpened)
            {
                Debug.Log("Middle button pressed!");
                mineField.ProceedOpenCellsAroundExceptFlagged(rowPos, colPos);
                mineField.DrawMineField();
                return;
            }

            return;
        }

        if (cell.IsOpened)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            cell.IsFlagged = !cell.IsFlagged;
            
            if (cell.IsFlagged)
            {
                mineField.ChangeMinesLeft(-1);
            }
            else
            {
                mineField.ChangeMinesLeft(1);
            }
            mineField.ReDrawCell(this, cell.IsFlagged);
            return;
        }

        if (cell.IsFlagged)
        {
            return;
        }
        

        int returnCode = cell.OpenCell();
        if (!gameStarted)
        {
            mineField.GameBegin();
        }

        gameStarted = true;


        mineField.ReDrawCell(this, returnCode);

        if (returnCode == -1)
        {
            mineField.GameLost();
            return;
        }
        else if (returnCode == 0)
        {
            mineField.OpenCellsAround(rowPos, colPos, true);
            mineField.DrawMineField();
        }

        mineField.AddOpenedCell();
    }
}
