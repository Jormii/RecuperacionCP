using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientsManager : MonoBehaviour
{
    public static ClientsManager INSTANCE;

    public int maxClientsPresentAtOnce = 1;
    [SerializeField] private List<Client> clientPrefabs = new List<Client>();

    private int clientsPresent;
    private HashSet<Client> clientsInMall;
    private List<Client> clientsCreated;

    void Start()
    {
        INSTANCE = this;

        clientsPresent = 0;
        clientsInMall = new HashSet<Client>();
        clientsCreated = new List<Client>();
    }

    void Update()
    {
        if (clientsPresent == maxClientsPresentAtOnce)
        {
            return;
        }

        clientsPresent += 1;
        Invoke("SpawnClient", Random.Range(0.5f, 1.5f));
    }

    private void SpawnClient()
    {
        List<LocationData> exitsLocations = Mall.INSTANCE.GetAllExits();
        int randomIndex = Random.Range(0, exitsLocations.Count - 1);

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
        int randomIndex = Random.Range(0, clientsCreated.Count - 1);
        Client client = clientsCreated[randomIndex];

        client.gameObject.SetActive(true);

        return client;
    }

    private Client CreateNewClient(LocationData spawnLocation)
    {
        int randomIndex = Random.Range(0, clientPrefabs.Count - 1);
        Client clientPrefab = clientPrefabs[randomIndex];
        Client newClient = Instantiate(clientPrefab, spawnLocation.POSITION, Quaternion.identity);

        return newClient;
    }

    public void ClientLeavesMall(Client client)
    {
        clientsPresent -= 1;
        clientsInMall.Remove(client);
        clientsCreated.Add(client);
    }
}
