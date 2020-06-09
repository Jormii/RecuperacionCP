using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Vision : MonoBehaviour
{

    public float viewDistance;
    private Human human;
    private Navigation navigation;

    void Start()
    {
        human = GetComponent<Human>();
        navigation = GetComponent<Navigation>();
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, navigation.direction, out hit, viewDistance))
        {
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
        ray.direction = navigation.direction;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(viewDistance * navigation.direction));
    }

}
