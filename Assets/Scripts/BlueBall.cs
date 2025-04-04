using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BlueBall : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private int duplicateCount = 3;
    public float force = 2f;
    private int clickCount = 0;

    // Composants
    private Animator m_animator;
    private Rigidbody2D m_rb;
    private CircleCollider2D m_cc;
    private Vector3 dragOffset;

    public bool isDragged = false;

    [Header("Particules/SFX")]
    public AudioClip as_duplicate;
    public AudioClip as_click;
    public GameObject duplicateParticules;
    public GameObject clickParticules;
    private AudioSource m_audioSource;

    [Header("Utils")]
    public bool isInhaled = false; // modifié par StockMachine
    public enum BlueBallState { Spawn, Idle, Click, Duplicate, Drag, Friction, Inhale }
    public BlueBallState currentState = BlueBallState.Spawn;

    [Header("Oscillation Settings")]
    public float amplitude = 5f;           // Amplitude verticale
    public float oscillationSpeed = 2f;      // Vitesse d'oscillation

    [Header("Friction Settings")]
    public float frictionFactor = 0.99f;
    public float frictionThreshold = 0.01f;

    // Variables d'oscillation
    private float originY;
    private float topY;
    private float bottomY;
    private int direction = 1; // 1 vers le haut, -1 vers le bas
    public bool isFreeze = false;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_cc = GetComponent<CircleCollider2D>();
        m_audioSource = GetComponent<AudioSource>();
        m_rb = GetComponent<Rigidbody2D>();

        // Démarrer en Spawn
        currentState = BlueBallState.Spawn;
        m_rb.velocity = Vector2.zero;
        StartCoroutine(TransitionFromSpawn());

        // Initialiser les bornes d'oscillation à la position actuelle
        originY = transform.position.y;
        topY = originY + amplitude;
        bottomY = originY - amplitude;
        direction = 1;
    }

    IEnumerator TransitionFromSpawn()
    {
        yield return new WaitForSeconds(1f);
        currentState = BlueBallState.Idle;
        // En Idle, la BlueBall oscille automatiquement
        m_rb.velocity = new Vector2(0, oscillationSpeed * direction);
    }

    void Update()
    {
        if (isInhaled)
        {
            currentState = BlueBallState.Inhale;
            m_animator.SetTrigger("Inhale");
            m_rb.velocity = Vector2.zero;
            return;
        }

        switch (currentState)
        {
            case BlueBallState.Spawn:
                m_rb.velocity = Vector2.zero;
                break;

            case BlueBallState.Idle:
                // Ici, Idle correspond à l'état Oscillating.
                // Gestion des inputs
                ClickEvent();
                // Oscillation verticale automatique
                transform.position += new Vector3(0, oscillationSpeed * direction * Time.deltaTime, 0);
                if (direction > 0 && transform.position.y >= topY)
                {
                    direction = -1;
                    m_rb.velocity = new Vector2(0, oscillationSpeed * direction);
                }
                else if (direction < 0 && transform.position.y <= bottomY)
                {
                    direction = 1;
                    m_rb.velocity = new Vector2(0, oscillationSpeed * direction);
                }
                break;

            case BlueBallState.Click:
                currentState = BlueBallState.Idle;
                break;

            case BlueBallState.Duplicate:

                currentState = BlueBallState.Idle;
                break;

            case BlueBallState.Drag:
                if (Input.GetMouseButton(1))
                {
                    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    transform.position = mouseWorldPos + (Vector2)dragOffset;
                    m_rb.velocity = Vector2.zero;
                }
                if (Input.GetMouseButtonUp(1) || isInhaled)
                {
                    currentState = BlueBallState.Idle;
                    GameManager.Instance.isDragging = false;
                    isDragged = false;
                    // Réinitialiser l'origine de l'oscillation à la position actuelle
                    originY = transform.position.y;
                    topY = originY + amplitude;
                    bottomY = originY - amplitude;
                }
                break;

            case BlueBallState.Friction:
                ClickEvent();
                m_rb.velocity *= frictionFactor;
                if (m_rb.velocity.magnitude < frictionThreshold)
                {
                    m_rb.velocity = Vector2.zero;
                    float distToTop = Mathf.Abs(topY - transform.position.y);
                    float distToBottom = Mathf.Abs(transform.position.y - bottomY);
                    direction = (distToTop < distToBottom) ? -1 : 1;
                    m_rb.velocity = new Vector2(0, oscillationSpeed * direction);
                    currentState = BlueBallState.Idle;
                }
                break;

            case BlueBallState.Inhale:
                m_rb.velocity = Vector2.zero;
                break;
        }

        if(isFreeze)
        {
            m_rb.velocity = Vector2.zero;
        }

    }

    // Vérifie si la souris est sur la BlueBall
    private bool IsMouseOver()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            return false;
        if (col is CircleCollider2D circle)
        {
            float radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            return Vector2.Distance(transform.position, mousePos) <= radius;
        }
        else
        {
            return col.OverlapPoint(mousePos);
        }
    }

    private void ClickEvent()
    {
        if (Input.GetMouseButtonDown(0) && IsMouseOver() && !GameManager.Instance.isDragging)
        {
            clickCount++;
            if (clickCount >= duplicateCount)
            {
                clickCount = 0;
                currentState = BlueBallState.Duplicate;
                m_audioSource.PlayOneShot(as_duplicate);
                m_animator.SetTrigger("Duplicate");
                Instantiate(duplicateParticules, transform.position, Quaternion.identity);
            }
            else
            {
                currentState = BlueBallState.Click;
                m_audioSource.PlayOneShot(as_click);
                m_animator.SetTrigger("Click");
                Instantiate(clickParticules, transform.position, Quaternion.identity);
            }
        }
        if (Input.GetMouseButtonDown(1) && IsMouseOver())
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragOffset = transform.position - (Vector3)mouseWorldPos;
            m_rb.velocity = Vector2.zero;
            GameManager.Instance.isDragging = true;
            currentState = BlueBallState.Drag;
            isDragged = true;
        }
    }

    private IEnumerator SpawnProp()
    {
        GameObject newObject = Instantiate(gameObject, transform.position, Quaternion.identity);
        newObject.name = gameObject.name;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Debug.Log(randomDir);
        newObject.GetComponent<Rigidbody2D>().AddForce(randomDir * force, ForceMode2D.Impulse);
        yield return null;
    }

    // Lorsqu'une collision avec un objet tagué "Ball" se produit, passage en Friction
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            currentState = BlueBallState.Friction;
        }
    }
}
