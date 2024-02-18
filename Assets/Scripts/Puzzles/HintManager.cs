using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public string message = "You need to place boxes of top of pressure plates";
    private bool open = false;

    public void interact()
    {
        open = !open;
        Time.timeScale = (open) ? 0f : 1f;

        if (open)
            Debug.Log(message);
    }
}
