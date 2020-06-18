using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{
    public static readonly Dictionary<int, Product> ALL_PRODUCTS = new Dictionary<int, Product>();

    public readonly int ID = IDProvider.GetID();
    [SerializeField] private string productName = "ProductName";

    private void Awake()
    {
        ALL_PRODUCTS.Add(ID, this);
        GetComponent<Product>().enabled = false;
    }

    public string ProductName
    {
        get => productName;
    }
}