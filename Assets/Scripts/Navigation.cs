using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    public float movementSpeed;

    public bool hasDestination;
    public bool hasReachedItsDestination;
    public GameObject destination;
    public Vector3 destinationPosition;
    public Vector2 direction;

    private void Update()
    {
        if (!hasDestination)
        {
            return;
        }

        // Can move only in X axis
        Vector2 pos = transform.position;
        float newX = pos.x + Time.deltaTime * movementSpeed * direction.x;
        Vector3 newPos = new Vector3(newX, pos.y, 0f);

        transform.position = newPos;

        if (ReachedDestination())
        {
            hasDestination = false;
            hasReachedItsDestination = true;
        }
    }

    public bool ReachedDestination()
    {
        float x = transform.position.x;
        float desX = destinationPosition.x;

        if (direction.x > 0)
        {
            return x >= desX;
        }
        else
        {
            return x <= desX;
        }
    }

    public bool MoveTo(GameObject gameObject)
    {
        if (hasDestination)
        {
            Debug.Log("Already moving somewhere else");
            return false;
        }

        Debug.Log("Moving towards " + gameObject);
        hasDestination = true;
        hasReachedItsDestination = false;
        destination = gameObject;
        destinationPosition = gameObject.transform.position;
        direction = (destinationPosition - transform.position).normalized;
        direction = (direction.x >= 0) ? Vector2.right : Vector2.left;
        return true;
    }

}
