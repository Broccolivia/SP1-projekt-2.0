using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.Tilemaps;
using UnityEngine;

public class ChasingEnemy : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;
    public float detectionRange = 5f;
    //[SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float bounciness = 100f;
    [SerializeField] private int damageGiven = 1;
    [SerializeField] private float knockbackForce = 100f;
    [SerializeField] private float upwardsForce = 5f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;
    private bool canChase = false;

    private SpriteRenderer spriteRenderer;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

   

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

       if (distanceToPlayer <= detectionRange)
        {
            canChase = true;
        }

       if (!canChase)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        spriteRenderer.flipX = direction < 0;

        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, 1 << player.gameObject.layer);

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);

            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);

            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 3f, groundLayer);

            if(!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }

                    
        }
    }

    private void FixedUpdate()
    {
        if(isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
       
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageGiven);

            if (other.transform.position.x > transform.position.x)
            {
                other.gameObject.GetComponent<PlayerMovement>().TakeKnockback(knockbackForce, upwardsForce);
            }
            else
            {
                other.gameObject.GetComponent<PlayerMovement>().TakeKnockback(-knockbackForce, upwardsForce);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rgbd = other.attachedRigidbody;

            if (rgbd != null)
            {
                rgbd.linearVelocity = new Vector2(rgbd.linearVelocity.x, 0);
                rgbd.AddForce(new Vector2(0, bounciness));
            }

            Destroy(gameObject);
        }
    }
}
