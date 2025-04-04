using UnityEngine;

public class Bumper : MonoBehaviour
{
    [Header("Bumper Settings")]
    // Force appliquée à la boule lors de la collision ou de l'initialisation
    public float bumpForce = 5f;
    // Vitesse de rotation par la molette de la souris
    public float rotationSpeed = 15f;

    // Composants
    private Rigidbody2D m_rb;
    private Animator animator;

    // Variables de drag
    private Vector3 dragOffset;
    private bool isDragged = false;
    private bool isMouseOver = false;

    // Variable statique locale pour limiter le drag à une seule instance
    private static bool isAnyObjectBeingDragged = false;

    // Machine à états
    private enum BumperState { Idle, Drag }
    private BumperState currentState = BumperState.Idle;

    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Supposons que le bumper soit orienté pour envoyer la boule vers le bas par défaut.
        // La direction de bump sera toujours définie par transform.up.
        // Vous pouvez ajuster la rotation initiale si nécessaire.
        // Exemple : transform.rotation = Quaternion.Euler(0, 0, 0);

        // Envoi initial de la boule vers le bas
        GameObject ball = GameObject.FindWithTag("Ball");
        if (ball != null)
        {
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.AddForce(transform.up * bumpForce, ForceMode2D.Impulse);
            }
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case BumperState.Idle:
                // Le drag ne démarre que si aucun autre objet n'est en cours de drag
                if (Input.GetMouseButtonDown(1) && isMouseOver && !isAnyObjectBeingDragged)
                {
                    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    dragOffset = transform.position - (Vector3)mouseWorldPos;
                    m_rb.velocity = Vector2.zero;

                    // Indiquer qu'un objet est en cours de drag
                    isAnyObjectBeingDragged = true;
                    currentState = BumperState.Drag;
                    isDragged = true;
                }
                break;

            case BumperState.Drag:
                // Rotation avec la molette pendant le drag
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.001f)
                {
                    float rotationAmount = scroll * rotationSpeed;
                    transform.Rotate(0, 0, rotationAmount);
                    // La direction de bump sera définie par transform.up après rotation
                }
                // Déplacement du bumper
                if (Input.GetMouseButton(1))
                {
                    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    transform.position = mouseWorldPos + (Vector2)dragOffset;
                    m_rb.velocity = Vector2.zero;
                }
                // Fin du drag
                if (Input.GetMouseButtonUp(1))
                {
                    currentState = BumperState.Idle;
                    isAnyObjectBeingDragged = false;
                    isDragged = false;
                }
                break;
        }
    }

    // Détection de collision avec la boule
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si l'objet est en cours de drag, ne pas exécuter le bump
        if (isDragged) return;

        // Vérifier que l'objet en collision possède le tag "Ball"
        if (collision.gameObject.CompareTag("Ball") || collision.gameObject.CompareTag("FirstBall"))
        {
            Rigidbody2D ballRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
            if (ballRigidbody != null)
            {
                animator.SetTrigger("IsBump");
                // La direction de la force est définie par transform.up,
                // qui évolue si vous faites tourner le bumper.
                Vector2 forceDirection = transform.up;
                ballRigidbody.AddForce(-forceDirection * bumpForce, ForceMode2D.Impulse);
            }
        }
    }

    // Détection du survol de la souris
    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        if (!isDragged)
        {
            isMouseOver = false;
        }
    }
}
