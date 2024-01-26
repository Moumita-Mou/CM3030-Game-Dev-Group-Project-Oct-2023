﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondPuzzleManager : MonoBehaviour
{
    [SerializeField] List<LightBulb> lightBulbs;
    [SerializeField] GameObject barrierPrefab;
    [SerializeField] Vector3 barrierPos;
    private GameObject barrier;

    private void Start()
    {
        spawnBarrier();
    }

    private void Update()
    {
        bool doorOpen = isDoorOpen();
        if (!doorOpen && barrier == null)
            spawnBarrier();

        if (doorOpen && barrier != null)
            despawnBarrier();
    }

    private bool isDoorOpen()
    {
        foreach (LightBulb lightBulb in lightBulbs)
            if (lightBulb.getState() == 0)
                return false;

        return true;
    }

    private void spawnBarrier()
    {
        barrier = Instantiate(barrierPrefab, barrierPos, Quaternion.identity);
    }

    private void despawnBarrier()
    {
        Destroy(barrier);
        barrier = null;
    }
}
