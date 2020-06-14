using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedTime : MonoBehaviour
{
    [SerializeField] private int openHour = 10;
    [SerializeField] private int closeHour = 22;

    private int openHourMinutes;
    private int closeHourMinutes;
    private float currentTime = 0f;
    private bool timeRunning = true;

    private void Start()
    {
        openHourMinutes = 60 * openHour;
        closeHourMinutes = 60 * closeHour;

        InvokeRepeating("OnNewHour", 60f, 60f);
    }

    private void Update()
    {
        if (timeRunning)
        {
            currentTime += Time.deltaTime;
        }
    }

    private void OnNewHour()
    {
        List<Store> allStores = Mall.INSTANCE.GetAllStores();
        for (int i = 0; i < allStores.Count; ++i)
        {
            allStores[i].OnNewHour();
        }

        Debug.LogWarning(ParseTime());
        if (GetTotalMinutes() >= closeHourMinutes)
        {
            OnCloseTimeMet();
        }
    }

    private void OnCloseTimeMet()
    {
        timeRunning = false;
        StopAllCoroutines();

        MakeClientsLeave();
        SendEmployeesHome();
        CloseMall();

        // TODO: Reopen mall

        Debug.LogWarning("MALL CLOSING");
    }

    private void MakeClientsLeave()
    {
        // TODO: Non efficient
        Client[] allClients = (Client[])GameObject.FindObjectsOfType(typeof(Client));
        for (int i = 0; i < allClients.Length; ++i)
        {
            allClients[i].MakeLeave();
        }
    }

    private void SendEmployeesHome()
    {
        List<Employee> allEmployees = Boss.INSTANCE.GetAllEmployees();
        for (int i = 0; i < allEmployees.Count; ++i)
        {
            allEmployees[i].SendHome();
        }
    }

    private void CloseMall()
    {
        List<Store> allStores = Mall.INSTANCE.GetAllStores();
        for (int i = 0; i < allStores.Count; ++i)
        {
            allStores[i].Close();
        }
    }

    private int GetTotalMinutes()
    {
        return openHourMinutes + (int)currentTime;
    }

    private int GetHour()
    {
        return GetTotalMinutes() / 60;
    }

    private int GetMinutes()
    {
        return GetTotalMinutes() % 60;
    }

    private string ParseTime()
    {
        int hour = GetHour();
        int minutes = GetMinutes();

        string hoursString = string.Format("{0}", hour).PadLeft(2, '0');
        string minutesString = string.Format("{0}", minutes).PadLeft(2, '0');
        string parsedString = string.Format("{0}:{1}", hoursString, minutesString);
        return parsedString;
    }
}
