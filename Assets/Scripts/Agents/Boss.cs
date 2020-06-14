using System.Collections.Generic;
using UnityEngine;

public class Boss
{
    public static readonly Boss INSTANCE = new Boss();

    private Dictionary<int, Employee> employees;

    private Boss()
    {
        this.employees = new Dictionary<int, Employee>();
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
}
