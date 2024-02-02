﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] Sprite defaultState;
    [SerializeField] Sprite pressedState;

    int state = 0;
    SpriteRenderer spriteRenderer;

    public int getState() {return state;}

    private void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        state = 1;
        spriteRenderer.sprite = pressedState;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        state = 0;
        spriteRenderer.sprite = defaultState;
    }
}