using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down
    };

    public readonly int ID = IDProvider.GetID();

    public int startingFloor;
    public Transform start;
    public int endingFloor;
    public Transform end;

    private LocationData startLocation;
    private LocationData endLocation;
    private Direction direction;

    void Start()
    {
        startLocation = new LocationData(start.position, startingFloor);
        endLocation = new LocationData(end.position, endingFloor);
        direction = (startingFloor < endingFloor) ? Direction.Up : Direction.Down;

        Mall.INSTANCE.AddStairs(this);
        GetComponent<Stairs>().enabled = false;
    }

    public LocationData StartingLocation
    {
        get => startLocation;
    }

    public LocationData EndingLocation
    {
        get => endLocation;
    }

    public Direction StairsDirection
    {
        get => direction;
    }

}
