﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Exit : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    [SerializeField] private int floor;

    private void Start()
    {
        Mall.INSTANCE.AddExit(this);
        GetComponent<Exit>().enabled = false;
    }

    public int Floor
    {
        get => floor;
    }
}