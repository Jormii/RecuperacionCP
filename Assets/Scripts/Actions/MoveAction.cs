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
        Navigation.Speed speedMode = (destination != Destination.NoDestination) ? Navigation.Speed.Normal : Navigation.Speed.Slow;
        if (destination != Destination.StairsEnd)
        {
            // In order to guarantee horizontal movement when not taking the stairs
            Vector2 currentPosition = navigation.transform.position;
            Vector2 destinationPosition = new Vector2(
                location.POSITION.x,
                currentPosition.y
            );

            navigation.MoveTo(destinationPosition, speedMode);
        }
        else
        {
            navigation.MoveTo(location.POSITION, speedMode);
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
