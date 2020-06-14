public struct ExitKnowledge
{
    public readonly int EXIT_ID;
    public readonly LocationData LOCATION;

    public ExitKnowledge(int id, LocationData location)
    {
        this.EXIT_ID = id;
        this.LOCATION = location;
    }
}
