using System.Threading;

public class IDProvider
{
    private static int idCounter = 0;

    public static int GetID()
    {
        return Interlocked.Increment(ref idCounter);
    }

}
