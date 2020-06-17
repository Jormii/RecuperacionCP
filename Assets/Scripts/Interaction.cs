﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    public Bubble bubblePrefab;

    private Bubble bubbleInstantiated;
    private bool bubbleExisting;

    private void Start()
    {
        bubbleExisting = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
            for (int i = 0; i < hits.Length; ++i)
            {
                GameObject gameObjectClicked = hits[i].transform.gameObject;
                Agent agent = gameObjectClicked.GetComponent<Agent>();
                if (agent)
                {
                    DrawBubble(agent);
                }
            }
        }
    }

    private void DrawBubble(Agent agent)
    {
        if (!bubbleExisting)
        {
            bubbleInstantiated = Instantiate<Bubble>(bubblePrefab, Vector3.zero, Quaternion.identity);
            bubbleExisting = true;
        }

        Vector3 bubblePosition = new Vector3(
            agent.transform.position.x + 0.7f,
            agent.transform.position.y + 0.7f,
            agent.transform.position.z
        );

        bubbleInstantiated.transform.position = bubblePosition;
        bubbleInstantiated.transform.parent = agent.transform;
    }
}
