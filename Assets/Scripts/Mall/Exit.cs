using UnityEngine;

public class Exit : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    public Transform spawnPosition;

    [SerializeField] private int floor;

    private void Awake()
    {
        Mall.INSTANCE.AddExit(this);
        GetComponent<Exit>().enabled = false;
    }

    public LocationData Location
    {
        get => new LocationData(spawnPosition.position, floor);
    }
}
