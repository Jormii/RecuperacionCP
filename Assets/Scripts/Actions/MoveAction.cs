using UnityEngine;

public class MoveAction : IAction
{
    public enum Destination
    {
        Agent,
        Exit,
        NoDestination,
        Stairs,
        StairsEnd,
        Storage,
        Store
    };

    private Navigation navigation;
    private LocationData location;
    private Destination destination;

    public MoveAction(Navigation navigation, LocationData location, Destination destination)
    {
        this.navigation = navigation;
        this.location = location;
        this.destination = destination;
    }

    public virtual void Execute()
    {
        if (destination != Destination.StairsEnd)
        {
            Vector2 currentPosition = navigation.transform.position;
            Vector2 destinationPosition = new Vector2(
                location.POSITION.x,
                currentPosition.y
            );

            navigation.MoveTo(destinationPosition);
        }
        else
        {
            navigation.MoveTo(location.POSITION);
        }
    }

    public void Cancel()
    {
        navigation.StopMoving();
    }

    public override string ToString()
    {
        return string.Format("MoveAction: ({0}, {1}) : {2}", location.POSITION, location.FLOOR, destination);
    }

    #region Properties

    public LocationData Location
    {
        get => location;
    }

    public Destination GetDestination
    {
        get => destination;
    }

    #endregion
}
