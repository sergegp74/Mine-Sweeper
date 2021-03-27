using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinesLeft : MonoBehaviour
{
    Text minesLeft = null;

    private void Awake()
    {
        minesLeft = GetComponent<Text>();
    }

    public void SetMinesLeft(int value)
    {
        Debug.Log(value);
        minesLeft.text = value.ToString();
    }
}
