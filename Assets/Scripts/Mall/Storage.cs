using UnityEngine;

public class Storage : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    public Transform entrance;

    [SerializeField] private int floor = 0;

    private void Awake()
    {
        Mall.INSTANCE.AddStorage(this);
        GetComponent<Storage>().enabled = false;
    }

    public LocationData Location
    {
        get => new LocationData(entrance.position, floor);
    }
}
