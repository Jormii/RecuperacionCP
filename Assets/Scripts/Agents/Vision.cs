using UnityEngine;

[RequireComponent(typeof(Agent))]
[RequireComponent(typeof(Navigation))]
public class Vision : MonoBehaviour
{
    [SerializeField] private float viewDistance = 2.5f;
    [SerializeField] private float visionTick = 0.25f;
    private Agent agent;
    private Navigation navigation;

    void Start()
    {
        agent = GetComponent<Agent>();
        navigation = GetComponent<Navigation>();
        InvokeRepeating("See", 0f, visionTick);
    }

    private void See()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, navigation.Direction, viewDistance);
        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit2D hit = hits[i];
            GameObject gameObjectHit = hit.transform.gameObject;

            Agent agentSeen = gameObjectHit.GetComponent<Agent>();
            if (agentSeen && agentSeen.CanInteractWith)
            {
                agent.OnOtherAgentSeen(agentSeen);
            }

            Store storeSeen = gameObjectHit.GetComponent<Store>();
            if (storeSeen)
            {
                agent.OnStoreSeen(storeSeen);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!navigation || !agent.CanInteractWith)
        {
            return;
        }

        Gizmos.color = Color.green;
        Ray ray = new Ray();
        ray.origin = transform.position;
        ray.direction = navigation.Direction;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(viewDistance * ray.direction));
    }
}
