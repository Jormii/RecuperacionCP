using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientsManager : MonoBehaviour
{
    public static ClientsManager INSTANCE;

    public float waitBetweeenSpawns = 1f;
    public int maxClientsPresentAtOnce = 1;
    [SerializeField] private List<Client> clientPrefabs = new List<Client>();

    private bool spawnClient;
    private int clientsPresent;
    private HashSet<Client> clientsInMall;
    private List<Client> clientsCreated;

    void Start()
    {
        INSTANCE = this;

        spawnClient = false;
        clientsPresent = 0;
        clientsInMall = new HashSet<Client>();
        clientsCreated = new List<Client>();

        Invoke("ResetSpawnClient", waitBetweeenSpawns);
    }

    void Update()
    {
        if (clientsPresent == maxClientsPresentAtOnce || !spawnClient)
        {
            return;
        }

        clientsPresent += 1;
        if (spawnClient)
        {
            Invoke("SpawnClient", Random.Range(0.5f, 1.5f));
            Invoke("ResetSpawnClient", waitBetweeenSpawns);
            spawnClient = false;
        }
    }

    private void SpawnClient()
    {
        List<LocationData> exitsLocations = Mall.INSTANCE.GetAllExits();
        int randomIndex = new System.Random().Next(0, exitsLocations.Count);

        LocationData spawnLocation = exitsLocations[randomIndex];

        Client client = null;

        int clientsCreatedCount = clientsCreated.Count;
        float threshold = 1f - 1f / (0.5f * clientsCreatedCount + 1f);
        float random = Random.Range(0f, 1f);

        if (random < threshold)
        {
            Debug.Log("Spawning a client that already visited the mall");
            client = SpawnAlreadyCreatedClient(spawnLocation);
        }
        else
        {
            Debug.Log("Spawning a new client");
            client = CreateNewClient(spawnLocation);
        }

        client.Reset(spawnLocation);
        clientsInMall.Add(client);
    }

    private Client SpawnAlreadyCreatedClient(LocationData spawnLocation)
    {
        int randomIndex = new System.Random().Next(0, clientsCreated.Count);
        Client client = clientsCreated[randomIndex];

        client.gameObject.SetActive(true);

        return client;
    }

    private Client CreateNewClient(LocationData spawnLocation)
    {
        int randomIndex = new System.Random().Next(0, clientPrefabs.Count);
        Client clientPrefab = clientPrefabs[randomIndex];
        Client newClient = Instantiate(clientPrefab, spawnLocation.POSITION, Quaternion.identity);

        return newClient;
    }

    private void ResetSpawnClient()
    {
        spawnClient = true;
    }

    public void ClientLeavesMall(Client client)
    {
        clientsPresent -= 1;
        clientsInMall.Remove(client);
        clientsCreated.Add(client);
    }
}
