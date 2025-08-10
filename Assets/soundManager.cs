using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManager : MonoBehaviour
{
    public static soundManager Instance { get; private set; }

    public AudioSource winBetLoseSource;
    public AudioSource envSource;

    public AudioClip winSource;
    public AudioClip loseSource;
    public AudioClip puttingBetSource;
    public AudioClip pleaseBetWomanSource;
    public AudioClip ballWheelSource;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
