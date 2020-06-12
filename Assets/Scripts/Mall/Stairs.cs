using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    [SerializeField] private int floor;

    void Start()
    {
        Mall.INSTANCE.AddStairs(this);
        Destroy(GetComponent<Stairs>());
    }

    public int Floor
    {
        get => floor;
    }
}
