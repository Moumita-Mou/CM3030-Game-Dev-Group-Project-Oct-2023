using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPuzzleManager : MonoBehaviour
{
    [SerializeField] List<ClickerPlate> clickerPlates;
    [SerializeField] GameObject barrierPrefab;
    [SerializeField] Vector3 barrierPos;

    private GameObject barrier;
    private int currentPlateIndex = 0;

    private void Start()
    {
        spawnBarrier();
    }

    public void setCurrentPlateIndex(int index)
    {
        if (index != currentPlateIndex)
        {
            foreach (ClickerPlate clickerPlate in clickerPlates)
                clickerPlate.resetPlate();

            currentPlateIndex = 0;
        }
        else
        {
            currentPlateIndex = index + 1;
            if (currentPlateIndex == clickerPlates.Count)
            {
                despawnBarrier();
            }
        }
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
