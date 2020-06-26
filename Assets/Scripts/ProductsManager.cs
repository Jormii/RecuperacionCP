using System.Collections.Generic;
using UnityEngine;

public class ProductsManager : MonoBehaviour
{
    public static ProductsManager INSTANCE;

    public List<Product> products = new List<Product>();

    private System.Random rng;

    private void Awake()
    {
        if (INSTANCE)
        {
            Debug.LogError("An instance of Products Manager already exists. Destroying...");
            Destroy(gameObject);
            return;
        }

        rng = new System.Random();
        INSTANCE = this;
    }

    public Product GetRandomProduct()
    {
        int randomIndex = rng.Next(0, products.Count);
        return products[randomIndex];
    }

    public List<Product> GetRandomProducts(int howMany)
    {
        List<Product> randomProducts = new List<Product>();
        List<Product> productsCopy = new List<Product>(products);

        for (int i = 0; i < howMany; ++i)
        {
            int randomIndex = rng.Next(0, productsCopy.Count);

            randomProducts.Add(productsCopy[randomIndex]);
            productsCopy.RemoveAt(randomIndex);
        }

        return randomProducts;
    }
}
