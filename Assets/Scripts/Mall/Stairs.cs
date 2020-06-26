using UnityEngine;

public class Stairs : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down
    };

    public readonly int ID = IDProvider.GetID();

    public int startingFloor = 0;
    public Transform start;
    public int endingFloor = 1;
    public Transform end;
    public SpriteRenderer frontSprite;
    public SpriteRenderer backSprite;

    private LocationData startLocation;
    private LocationData endLocation;
    private Direction direction;

    void Awake()
    {
        startLocation = new LocationData(start.position, startingFloor);
        endLocation = new LocationData(end.position, endingFloor);
        direction = (startingFloor < endingFloor) ? Direction.Up : Direction.Down;

        Mall.INSTANCE.AddStairs(this);
    }

    private void Start()
    {
        int max = Mathf.Max(startingFloor, endingFloor) * 10;

        frontSprite.sortingOrder = -(max - 3);
        backSprite.sortingOrder = -(max - 1);

        GetComponent<Stairs>().enabled = false;
    }

    public LocationData StartingLocation
    {
        get => startLocation;
    }

    public LocationData EndingLocation
    {
        get => endLocation;
    }

    public Direction StairsDirection
    {
        get => direction;
    }
}
