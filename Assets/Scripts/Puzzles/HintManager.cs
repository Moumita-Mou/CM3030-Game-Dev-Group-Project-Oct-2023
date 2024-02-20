using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HintManager : MonoBehaviour
{
    public string message = "You need to place boxes of top of pressure plates";
    public GameObject hintCanvas;
    public TextMeshProUGUI tmp;
    private bool open = false;

    private void Start()
    {
        hintCanvas.SetActive(false);
    }


    public void interact()
    {
        open = !open;
        Time.timeScale = (open) ? 0f : 1f;
        hintCanvas.SetActive(open);

        if (open)
            tmp.text = message;
    }
}
