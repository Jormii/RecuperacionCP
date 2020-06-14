using UnityEngine;

public class Storage : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();

    [SerializeField] private int floor = 0;

    private void Start()
    {
        Mall.INSTANCE.AddStorage(this);

        GetComponent<Storage>().enabled = false;
    }

    public int Floor
    {
        get => floor;
    }
}
