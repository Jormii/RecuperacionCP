public class MoveToStoreAction : MoveAction
{
    public readonly int STORE_ID;

    public MoveToStoreAction(Navigation navigation, LocationData location, int storeID) :
        base(navigation, location, Destination.Store)
    {
        this.STORE_ID = storeID;
    }

}
