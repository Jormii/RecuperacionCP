using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour
{
    private const float BUBBLE_X_OFFSET = 0.7f;
    private const float BUBBLE_Y_OFFSET = 0.7f;

    public Bubble bubblePrefab;

    private Agent activeAgent;
    private Bubble bubbleInstantiated;
    private bool bubbleExisting;

    private void Start()
    {
        bubbleExisting = false;
    }

    private void Update()
    {
        HandleInput();
        if (activeAgent)
        {
            UpdateBubble();
        }
    }

    private void HandleInput()
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
                    activeAgent = agent;
                }
            }
        }
    }

    private void UpdateBubble()
    {
        if (!bubbleExisting)
        {
            bubbleInstantiated = Instantiate<Bubble>(bubblePrefab, Vector3.zero, Quaternion.identity);
            bubbleExisting = true;
        }

        List<Sprite> sprites = activeAgent.GetSpritesToDisplay();
        if (sprites.Count == 0)
        {
            return;
        }

        Vector3 bubblePosition = new Vector3(
            activeAgent.transform.position.x + BUBBLE_X_OFFSET,
            activeAgent.transform.position.y + BUBBLE_Y_OFFSET,
            activeAgent.transform.position.z
        );

        bubbleInstantiated.transform.position = bubblePosition;
        bubbleInstantiated.transform.SetParent(activeAgent.transform);
        bubbleInstantiated.Draw(sprites);
    }
}
