using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Store : MonoBehaviour
{
    public StoreData storeData;
    public Stock stock;

    private void Start()
    {
        storeData.position = transform.position;
        stock = GetComponent<Stock>();

        Mall.INSTANCE.AddStore(this);
    }

    public void Sell(Product product, int amount)
    {
        // TODO
    }

}
