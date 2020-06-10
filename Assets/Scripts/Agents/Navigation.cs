using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Human))]
public class Navigation : MonoBehaviour
{
    public const float DISTANCE_MARGIN = 0.1f;

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
            thisComponent.enabled = false;
            human.UponReachingDestination();
        }
    }

    public bool ReachedItsDestination()
    {
        Vector2 pos = new Vector2(transform.position.x, 0f);
        Vector2 desPos = new Vector2(destination.x, 0f);
        float d = Vector2.Distance(pos, desPos);
        return Mathf.Abs(d) < DISTANCE_MARGIN;
    }

    public void MoveTo(Vector2 position)
    {
        thisComponent.enabled = true;

        destination = position;
        direction = (position - new Vector2(transform.position.x, transform.position.y)).normalized;
        direction = (direction.x >= 0) ? Vector2.right : Vector2.left;
    }

    public void StopMoving()
    {
        thisComponent.enabled = false;
    }

    #region Properties

    public Vector2 Direction
    {
        get => direction;
    }

    #endregion

}
