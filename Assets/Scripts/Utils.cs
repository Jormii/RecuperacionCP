using UnityEngine;

public class Utils
{
    public static float ManhattanDistance(Vector2 A, Vector2 B)
    {
        return Mathf.Abs(A.x - B.x) + Mathf.Abs(A.y - B.y);
    }
}
