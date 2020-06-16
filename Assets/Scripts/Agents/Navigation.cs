using UnityEngine;

[RequireComponent(typeof(Agent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Navigation : MonoBehaviour
{
    public const float DISTANCE_MARGIN = 0.1f;

    [SerializeField] private float movementSpeed = 2f;

    private Agent agent;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 destination;
    private Vector2 direction;
    private Navigation thisComponent;
    private float constantZ;

    private void Start()
    {
        agent = GetComponent<Agent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        thisComponent = GetComponent<Navigation>();
        constantZ = transform.position.z;
    }

    private void Update()
    {
        Vector2 currentPosition = transform.position;
        Vector2 newPosition2D = currentPosition + Time.deltaTime * movementSpeed * direction;

        Vector3 newPos = new Vector3(newPosition2D.x, newPosition2D.y, constantZ);
        transform.position = newPos;

        if (ReachedItsDestination())
        {
            thisComponent.enabled = false;
            animator.SetBool("moving", true);
            agent.UponReachingDestination();
        }
    }

    public bool ReachedItsDestination()
    {
        Vector2 currentPosition = transform.position;
        float distanceToDestination = Vector2.Distance(currentPosition, destination);
        return Mathf.Abs(distanceToDestination) < DISTANCE_MARGIN;
    }

    public void MoveTo(Vector2 position)
    {
        thisComponent.enabled = true;
        Vector2 currentPosition2D = transform.position;

        destination = position;
        direction = (position - currentPosition2D).normalized;

        bool movingRight = direction.x > 0;
        bool movingVertically = direction.y != 0;
        spriteRenderer.flipX = movingRight;
        // animator.SetBool("moving", !movingVertically);
        animator.SetBool("moving", true);
    }

    public void StopMoving()
    {
        thisComponent.enabled = false;
        animator.SetBool("moving", false);
    }

    #region Properties

    public Vector2 Destination
    {
        get => destination;
    }

    public Vector2 Direction
    {
        get => direction;
    }

    #endregion
}
