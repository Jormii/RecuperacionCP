using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Human))]
[RequireComponent(typeof(Navigation))]
public class Vision : MonoBehaviour
{
    private const float VISION_TICK = 0.5f;

    [SerializeField] private float viewDistance = 2.5f;
    private Human human;
    private Navigation navigation;

    void Start()
    {
        human = GetComponent<Human>();
        navigation = GetComponent<Navigation>();
        InvokeRepeating("See", 0f, VISION_TICK);
    }

    private void See()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, navigation.Direction, viewDistance);
        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit hit = hits[i];
            GameObject gameObjectHit = hit.transform.gameObject;
            Store store = gameObjectHit.GetComponent<Store>();
            if (store)
            {
                human.OnStoreSeen(store);
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
