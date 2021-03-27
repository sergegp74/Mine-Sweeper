using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    Text timerValue = null;
    float timer = 0;
    bool timerStarted = false;

    private void Awake()
    {
        timerValue = GetComponent<Text>();
    }

    void Start()
    {
        timerValue.text = "0";
    }

    void Update()
    {
        if (timerStarted)
        {
            timer += Time.deltaTime;
            timerValue.text = timer.ToString();
        }
    }

    public void ClearTimer()
    {
        timerStarted = false;
        timerValue.text = "0";
        timer = 0;
        CellUI.gameStarted = false;
    }

    public void StartTimer()
    {
        timerStarted = true;
    }

    public void StopTimer()
    {
        timerStarted = false;
    }
}
