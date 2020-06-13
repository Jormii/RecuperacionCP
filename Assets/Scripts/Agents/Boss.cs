using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss
{
    public static readonly Boss INSTANCE = new Boss();

    private Dictionary<int, Employee> employees;

    private Boss()
    {
        employees = new Dictionary<int, Employee>();
    }

    public bool AddEmployee(Employee employee)
    {
        int employeeID = employee.GetInstanceID();
        if (employees.ContainsKey(employeeID))
        {
            return false;
        }

        employees.Add(employeeID, employee);
        return true;
    }

    public void RequestReStock(Store store, Dictionary<int, int> reStock)
    {
        foreach (Employee employee in employees.Values)
        {
            if (employee.CanBeInterrupted())
            {
                employee.SendToReStock(store, reStock);
            }
        }
    }

}
