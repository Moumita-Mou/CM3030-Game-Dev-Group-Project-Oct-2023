using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAudio : MonoBehaviour
{
    [Header("Game Over")]
    [SerializeField]
    AudioSource gameOverSFX;

    [Header("Wave Alert")]
    [SerializeField]
    public AudioSource incomingWaveSFX;

    [Header("Combat Music")]
    [SerializeField]
    public AudioSource combatMusic;

    [Header("Background Music")]
    [SerializeField]
    public AudioSource outOfCombatMusic;

    public void gameOverSound()
    {
        //Debug.Log("Sound triggered");
        combatMusic.Stop();
        outOfCombatMusic.Stop();
        gameOverSFX.Play();
    }

    public void WaveAlertSound()
    {
        //Debug.Log("Sound triggered");
        //combatMusic.Stop();
        //outOfCombatMusic.Stop();
        incomingWaveSFX.Play();
    }

    public void playCombatMusic()
    {
        //Debug.Log("Sound triggered");
        outOfCombatMusic.Stop();
        combatMusic.Play();
    }

    public void playBackgroundMusic()
    {
        //Debug.Log("Sound triggered");
        combatMusic.Stop();
        outOfCombatMusic.Play();
    }
}
