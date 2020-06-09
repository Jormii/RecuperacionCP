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
        NoDestination
    }

    public Navigation navigation;
    public Vector2 position;
    public Destination destination;

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

}
