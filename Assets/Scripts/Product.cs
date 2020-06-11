using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product : MonoBehaviour
{
    public readonly int ID = IDProvider.GetID();
    [SerializeField] private string productName = "ProductName";

    public string ProductName
    {
        get => productName;
    }

}