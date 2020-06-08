using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoreData
{
    public int floor;
    public Vector2 position;

    public StoreData()
    {
        this.floor = 0;
        this.position = new Vector2();
    }
}
