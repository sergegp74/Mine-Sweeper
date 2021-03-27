using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private bool isOpened = false;
    private bool hasMine = false;
    private int minesAround = -1;
    private bool isFlagged = false;

    public Cell(int minesAround)
    {
        this.minesAround = minesAround;
    }
    public Cell(bool hasMine)
    {
        this.hasMine = hasMine;
    }

    public bool HasMine => hasMine;

    public bool IsOpened => isOpened;

    public int MinesAround => minesAround;

    public bool IsFlagged { get => isFlagged; set => isFlagged = value; }

    public int OpenCell()
    {
        isOpened = true;

        if (HasMine)
        {
            return -1;
            //Explode();
            //FinishGame();
        }
        else
        {
            return MinesAround;
        }
    }
}
