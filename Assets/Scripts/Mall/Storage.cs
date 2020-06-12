using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Storage : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    [SerializeField] private int floor;

    private void Start()
    {
        Mall.INSTANCE.AddStorage(this);
        Destroy(GetComponent<Storage>());
    }

    public int Floor
    {
        get => floor;
    }
}
