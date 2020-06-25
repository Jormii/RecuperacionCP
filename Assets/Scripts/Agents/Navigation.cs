using UnityEngine;

[RequireComponent(typeof(Agent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Navigation : MonoBehaviour
{
    public enum Speed
    {
        Normal,
        Slow
    };

    public const float DISTANCE_MARGIN = 0.1f;

    [SerializeField] private float movementSpeed = 2f;

    private Agent agent;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 destination;
    private Vector2 direction;
    private Navigation thisComponent;
    private Speed currentSpeedMode;
    private float constantZ;

    private void Awake()
    {
        agent = GetComponent<Agent>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        thisComponent = GetComponent<Navigation>();
        constantZ = transform.position.z;
    }

    private void Update()
    {
        float speed = (currentSpeedMode == Speed.Normal) ? movementSpeed : 0.66f * movementSpeed;
        Vector2 currentPosition = transform.position;
        Vector2 newPosition2D = currentPosition + Time.deltaTime * speed * direction;

        Vector3 newPos = new Vector3(newPosition2D.x, newPosition2D.y, constantZ);
        transform.position = newPos;

        if (ReachedItsDestination())
        {
            thisComponent.enabled = false;
            animator.SetBool("moving", false);
            agent.UponReachingDestination();
        }
    }

    public bool ReachedItsDestination()
    {
        Vector2 currentPosition = transform.position;
        float distanceToDestination = Vector2.Distance(currentPosition, destination);
        return Mathf.Abs(distanceToDestination) < DISTANCE_MARGIN;
    }

    public void MoveTo(Vector2 position, Speed speedMode)
    {
        thisComponent.enabled = true;
        currentSpeedMode = speedMode;
        Vector2 currentPosition2D = transform.position;

        destination = position;
        direction = (position - currentPosition2D).normalized;

        bool movingRight = direction.x > 0;
        bool movingVertically = direction.y != 0;
        spriteRenderer.flipX = movingRight;
        animator.SetBool("moving", true);
    }

    public void StopMoving()
    {
        animator.SetBool("moving", false);
        thisComponent.enabled = false;
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
