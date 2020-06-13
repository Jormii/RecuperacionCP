using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    public int startingFloor;
    public Transform start;
    public int endingFloor;
    public Transform end;

    private LocationData startLocation;
    private LocationData endLocation;

    void Start()
    {
        startLocation = new LocationData(start.position, startingFloor);
        endLocation = new LocationData(end.position, endingFloor);

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
}
