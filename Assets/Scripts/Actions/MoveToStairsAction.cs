using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToStairsAction : MoveAction
{
    private LocationData spawnLocation;

    public MoveToStairsAction(Navigation navigation, LocationData stairsLocation, LocationData spawnLocation) :
    base(navigation, stairsLocation, Destination.Stairs)
    {
        this.spawnLocation = spawnLocation;
    }

    public LocationData SpawnLocation
    {
        get => spawnLocation;
    }

}
