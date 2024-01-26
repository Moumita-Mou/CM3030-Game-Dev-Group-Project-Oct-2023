using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    [SerializeField] List<LightBulb> lightBulbs;

    private int state = 0;

    public void changeState()
    {
        state = (state + 1) % 2;

        foreach (LightBulb manager in lightBulbs)
            manager.changeState();

        float angle = 45 * ((state == 0) ? 1 : -1);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
