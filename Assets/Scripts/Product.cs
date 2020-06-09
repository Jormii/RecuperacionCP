using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{
    [SerializeField] private string productName = "Product";
    [SerializeField] private int defaultPrice = 0;

    public override int GetHashCode()
    {
        return productName.GetHashCode();
    }

    public string ProductName
    {
        get => productName;
    }

    public int DefaultPrice
    {
        get => defaultPrice;
    }

}