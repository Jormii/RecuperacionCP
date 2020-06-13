using UnityEngine;

public struct LocationData
{
    public readonly Vector2 POSITION;
    public readonly int FLOOR;

    public LocationData(Vector2 position, int floor)
    {
        this.POSITION = position;
        this.FLOOR = floor;
    }
}
