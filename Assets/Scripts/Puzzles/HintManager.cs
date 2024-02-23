using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Scripts;

public class HintManager : MonoBehaviour
{
    public string message = "You need to place boxes of top of pressure plates";

    public void interact()
    {
        FindObjectOfType<GameplayManager>().OnHintInteraction(message);
    }
}
