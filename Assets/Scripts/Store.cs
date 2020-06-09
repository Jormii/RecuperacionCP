using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Store : MonoBehaviour
{
    private static int idCounter = 0;

    public readonly int ID = GetStoreID();
    public int floor;
    public Stock stock;

    private void Start()
    {
        stock = GetComponent<Stock>();

        Mall.INSTANCE.AddStore(this);
    }

    public void Sell(Product product, int amount)
    {
        // TODO
    }

    private static int GetStoreID()
    {
        return Interlocked.Increment(ref idCounter);
    }

    public override int GetHashCode()
    {
        return ID;
    }

}
