using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductsManager : MonoBehaviour
{
    public static ProductsManager INSTANCE;

    public List<Product> products = new List<Product>();

    private void Awake()
    {
        INSTANCE = this;
    }

    public List<Product> GetRandomProducts(int howMany)
    {
        List<Product> productsCopy = new List<Product>(products);

        System.Random rng = new System.Random();
        List<Product> randomProducts = new List<Product>();
        for (int i = 0; i < howMany; ++i)
        {
            int randomIndex = rng.Next(0, productsCopy.Count);

            randomProducts.Add(productsCopy[randomIndex]);
            productsCopy.RemoveAt(randomIndex);
        }

        return randomProducts;
    }
}
