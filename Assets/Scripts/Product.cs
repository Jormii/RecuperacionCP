using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{
    public string productName = "Product";
    public int defaultPrice = 0;

    public override int GetHashCode()
    {
        return productName.GetHashCode();
    }

}