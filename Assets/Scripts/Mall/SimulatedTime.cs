using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimulatedTime : MonoBehaviour
{
    public TextMeshProUGUI UIText;

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

        UIText.text = ParseTime();
    }

    private void OnNewHour()
    {
        List<Store> allStores = Mall.INSTANCE.GetAllStores();
        for (int i = 0; i < allStores.Count; ++i)
        {
            allStores[i].OnNewHour();
        }

        int totalMinutes = GetTotalMinutes();
        bool isCloseTime = Mathf.Abs(totalMinutes - closeHourMinutes) <= 2;
        if (isCloseTime)
        {
            OnCloseTimeMet();
        }
    }

    private void OnCloseTimeMet()
    {
        timeRunning = false;

        currentTime = closeHourMinutes - openHourMinutes;
        CancelInvoke();

        MakeClientsLeave();
        SendEmployeesHome();
        CloseMall();

        GetComponent<SimulatedTime>().enabled = false;
    }

    private void MakeClientsLeave()
    {
        List<Client> allClients = ClientsManager.INSTANCE.GetAllClientsInMall();
        for (int i = 0; i < allClients.Count; ++i)
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
        Mall.INSTANCE.Close();
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
