using System.Collections.Generic;
using UnityEngine;

public class Boss
{
    public static readonly Boss INSTANCE = new Boss();

    private Dictionary<int, Employee> employees;
    private Dictionary<int, SalesReport> storeSalesPreviousHour;

    private Boss()
    {
        this.employees = new Dictionary<int, Employee>();
        this.storeSalesPreviousHour = new Dictionary<int, SalesReport>();
    }

    public void AddEmployee(Employee employee)
    {
        int employeeID = employee.GetInstanceID();
        if (employees.ContainsKey(employeeID))
        {
            return;
        }

        employees.Add(employeeID, employee);
    }

    public List<Employee> GetAllEmployees()
    {
        return new List<Employee>(employees.Values);
    }

    public void RequestReStock(Store store, Dictionary<int, int> reStock)
    {
        List<Employee> employeesAvailable = new List<Employee>();
        foreach (Employee employee in employees.Values)
        {
            if (employee.CanBeInterrupted() && employee.InChargeOfFloor(store.Location.FLOOR))
            {
                employeesAvailable.Add(employee);
            }
        }

        if (employeesAvailable.Count == 0)
        {
            Debug.LogWarning("No employees available for restocking");
            return;
        }

        Employee closestEmployee = null;
        float distanceToClosest = Mathf.Infinity;
        for (int i = 0; i < employeesAvailable.Count; ++i)
        {
            Employee employee = employeesAvailable[i];
            LocationData employeeLocation = employee.Location;
            LocationData closestStorageToEmployee = Mall.INSTANCE.GetClosestStorage(employeeLocation);

            float distanceToStorage = Utils.ManhattanDistance(employeeLocation.POSITION, closestStorageToEmployee.POSITION);
            float distanceToStore = Utils.ManhattanDistance(employeeLocation.POSITION, store.Location.POSITION);
            float totalDistance = 2f * distanceToStorage + distanceToStore;
            if (totalDistance < distanceToClosest)
            {
                closestEmployee = employee;
                distanceToClosest = totalDistance;
            }
        }

        closestEmployee.SendToReStock(store, reStock);
    }

    #region Stock Modification

    public StockChanges SendSalesReport(SalesReport salesReport)
    {
        StockChanges changes = new StockChanges(salesReport.STORE_ID);

        // Very first hour. No info about store
        if (!storeSalesPreviousHour.ContainsKey(salesReport.STORE_ID))
        {
            storeSalesPreviousHour.Add(salesReport.STORE_ID, salesReport);
        }
        else
        {
            changes = EvaluateChanges(salesReport);
            storeSalesPreviousHour[salesReport.STORE_ID] = salesReport;
        }

        return changes;
    }

    private StockChanges EvaluateChanges(SalesReport salesReport)
    {
        StockChanges changes = new StockChanges(salesReport.STORE_ID);
        SalesReport previousSalesReport = storeSalesPreviousHour[salesReport.STORE_ID];

        bool increasedProfit = salesReport.PROFIT > previousSalesReport.PROFIT;

        Dictionary<int, int> sales = salesReport.PRODUCTS_SOLD;
        Dictionary<int, int> reStock = salesReport.TIMES_RESTOCKED;
        Dictionary<int, int> previousSales = previousSalesReport.PRODUCTS_SOLD;
        Dictionary<int, int> previousReStock = previousSalesReport.TIMES_RESTOCKED;
        foreach (int productID in salesReport.PRODUCTS_SOLD.Keys)
        {
            int amountSold = sales[productID];
            int amountReStocked = reStock[productID];

            if (!previousSales.ContainsKey(productID))
            {
                // No info on this product to decide
                continue;
            }

            int previousAmountSold = previousSales[productID];
            int previousAmountReStocked = previousReStock[productID];

            bool moreProductsSold = amountSold > previousAmountSold;
            bool moreTimesReStocked = amountReStocked > previousAmountReStocked;

            if (moreProductsSold && moreTimesReStocked)
            {
                changes.ChangePrice(productID, (increasedProfit) ? 5 : 2);
                changes.ChangeStock(productID, (increasedProfit) ? 3 : 2);
            }
            else if (moreProductsSold)
            {
                changes.ChangeStock(productID, (increasedProfit) ? 3 : 2);
            }
            else if (moreTimesReStocked)
            {
                changes.ChangePrice(productID, (increasedProfit) ? 5 : 2);
                changes.ChangeStock(productID, (increasedProfit) ? 3 : 2);
            }
            else
            {
                if (!increasedProfit)
                {
                    changes.RemoveProduct(productID);
                    // changes.NewProduct();   // TODO
                }
                else
                {
                    changes.ChangeStock(productID, -2);
                    changes.ChangePrice(productID, -5);
                }
            }
        }

        return changes;
    }

    #endregion
}
