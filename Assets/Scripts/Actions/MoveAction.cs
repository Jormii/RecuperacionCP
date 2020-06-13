public class MoveAction : IAction
{
    public enum Destination
    {
        Agent,
        Store,
        Stairs,
        StairsEnd,
        Exit,
        Storage,
        NoDestination
    }

    private Navigation navigation;
    private LocationData location;
    private Destination destination;

    public MoveAction(Navigation navigation, LocationData location, Destination destination)
    {
        this.navigation = navigation;
        this.location = location;
        this.destination = destination;
    }

    public void Execute()
    {
        navigation.MoveTo(location.POSITION);
    }

    public void Cancel()
    {
        navigation.StopMoving();
    }

    #region Properties

    public bool CanBeCancelled
    {
        get => true;
    }

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
