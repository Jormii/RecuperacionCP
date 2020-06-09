using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Exit : MonoBehaviour
{

    private static int idCounter = 0;
    [SerializeField] private int floor;

    public readonly int ID = GetExitID();

    private void Start()
    {
        Mall.INSTANCE.AddExit(this);
    }

    public int Floor
    {
        get => floor;
    }

    private static int GetExitID()
    {
        return Interlocked.Increment(ref idCounter);
    }
}
