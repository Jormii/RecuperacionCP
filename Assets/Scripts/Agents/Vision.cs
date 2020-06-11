using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        RaycastHit[] hits = Physics.RaycastAll(transform.position, navigation.Direction, viewDistance);
        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit hit = hits[i];
            GameObject gameObjectHit = hit.transform.gameObject;

            Agent agentSeen = gameObjectHit.GetComponent<Agent>();
            if (agentSeen)
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
        if (!navigation)
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
