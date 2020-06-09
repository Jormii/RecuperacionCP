using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ExitKnowledge
{
    public readonly int EXIT_ID;
    public readonly int FLOOR;
    public readonly Vector2 POSITION;

    public ExitKnowledge(int id, int floor, Vector2 position)
    {
        this.EXIT_ID = id;
        this.FLOOR = floor;
        this.POSITION = position;
    }

}
