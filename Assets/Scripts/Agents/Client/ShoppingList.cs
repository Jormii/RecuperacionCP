using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShoppingList
{
    [SerializeField] private Product product;
    [SerializeField] private int quantityWanted;
    [SerializeField] private int quantityBought;

    public ShoppingList(Product product, int quantityWanted)
    {
        this.product = product;
        this.quantityWanted = quantityWanted;
        this.quantityBought = 0;
    }

    public bool PurchaseComplete()
    {
        return quantityWanted == quantityBought;
    }

    public int LeftToBuy()
    {
        return quantityWanted - quantityBought;
    }

    public void Buy(int amount)
    {
        quantityBought += amount;
    }

    #region Properties

    public Product Product
    {
        get => product;
    }

    public int QuantityWanted
    {
        get => quantityWanted;
    }

    public int QuantityBought
    {
        get => quantityBought;
    }

    #endregion

}
