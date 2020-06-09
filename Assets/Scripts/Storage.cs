using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Storage : MonoBehaviour
{
    private static int idCounter = 0;
    [SerializeField] private int floor;

    public readonly int ID = GetStorageID();

    private void Start()
    {
        Mall.INSTANCE.AddStorage(this);
    }

    public int Floor
    {
        get => floor;
    }

    private static int GetStorageID()
    {
        return Interlocked.Increment(ref idCounter);
    }
}
