using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    [SerializeField] private float movementSpeed;

    private Human human;
    private Vector2 destination;
    private Vector2 direction;
    private Navigation thisComponent;

    private void Start()
    {
        human = GetComponent<Human>();
        thisComponent = GetComponent<Navigation>();
    }

    private void Update()
    {
        // Can move only in the X axis
        Vector2 pos = transform.position;
        float newX = pos.x + Time.deltaTime * movementSpeed * direction.x;
        Vector3 newPos = new Vector3(newX, pos.y, 0f);

        transform.position = newPos;

        if (ReachedItsDestination())
        {
            human.UponReachingDestination();
            thisComponent.enabled = false;
        }
    }

    public bool ReachedItsDestination()
    {
        float x = transform.position.x;
        float desX = destination.x;

        if (direction.x > 0)
        {
            return x >= desX;
        }
        else
        {
            return x <= desX;
        }
    }

    public void MoveTo(Vector2 position)
    {
        thisComponent.enabled = true;

        destination = position;
        direction = (position - new Vector2(transform.position.x, transform.position.y)).normalized;
        direction = (direction.x >= 0) ? Vector2.right : Vector2.left;
    }

    #region Properties

    public Vector2 Direction
    {
        get => direction;
    }

    #endregion

}
