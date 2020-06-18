using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{
    private static readonly Dictionary<int, Product> ALL_PRODUCTS = new Dictionary<int, Product>();

    public readonly int ID = IDProvider.GetID();
    [SerializeField] private string productName = "ProductName";

    private void Awake()
    {
        ALL_PRODUCTS.Add(ID, this);
        gameObject.SetActive(false);
    }

    public static List<Product> GetRandomProducts(int howMany)
    {
        List<int> keys = new List<int>();
        foreach (int productID in ALL_PRODUCTS.Keys)
        {
            keys.Add(productID);
        }

        List<Product> products = new List<Product>();
        for (int i = 0; i < howMany; ++i)
        {
            int randomIndex = Random.Range(0, keys.Count - 1);
            int key = keys[randomIndex];

            products.Add(ALL_PRODUCTS[key]);
            keys.RemoveAt(randomIndex);
        }

        return products;
    }

    public string ProductName
    {
        get => productName;
    }
}