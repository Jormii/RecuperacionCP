using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : IAction
{
    public enum Destination
    {
        Store,
        Stairs,
        Exit,
        Storage,
        NoDestination
    }

    private Navigation navigation;
    private Vector2 position;
    private Destination destination;

    public MoveAction(Navigation navigation, Vector2 position, Destination destination)
    {
        this.navigation = navigation;
        this.position = position;
        this.destination = destination;
    }

    public void Execute()
    {
        navigation.MoveTo(position);
    }

    public void Cancel()
    {
        navigation.StopMoving();
    }

    public Destination GetDestination
    {
        get => destination;
    }

}
