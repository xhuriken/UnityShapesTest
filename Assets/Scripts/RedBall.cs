using Shapes;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class RedBall : MonoBehaviour
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

    // Gestion du drag
    public bool isDragged = false;

    [Header("Particules/SFX")]
    public AudioClip as_duplicate;
    public AudioClip as_click;
    public GameObject duplicateParticules;
    public GameObject clickParticules;
    private AudioSource m_audioSource;

    [Header("Utils")]
    public bool isInhaled = false; // modifié par StockMachine.cs

    // State machine
    public enum RedBallState { Spawn, Idle, Click, Duplicate, Drag, Inhale }
    public RedBallState currentState = RedBallState.Spawn;

    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_cc = GetComponent<CircleCollider2D>();
        m_audioSource = GetComponent<AudioSource>();
        m_rb = GetComponent<Rigidbody2D>();

        // Commencer en Spawn: immobile pendant 1 seconde
        currentState = RedBallState.Spawn;
        m_rb.velocity = Vector2.zero;
        StartCoroutine(TransitionFromSpawn());
    }

    IEnumerator TransitionFromSpawn()
    {
        yield return new WaitForSeconds(1f);
        currentState = RedBallState.Idle;
    }

    void Update()
    {
        switch (currentState)
        {
            case RedBallState.Spawn:
                m_rb.velocity = Vector2.zero;
                break;

            case RedBallState.Idle:
                if (isInhaled)
                {
                    currentState = RedBallState.Inhale;
                    m_animator.SetTrigger("Inhale");
                    m_rb.velocity = Vector2.zero;
                    break;
                }
                if (Input.GetMouseButtonDown(0) && IsMouseOver() && !GameManager.Instance.isDragging)
                {
                    clickCount++;
                    if (clickCount >= duplicateCount)
                    {
                        clickCount = 0;
                        currentState = RedBallState.Duplicate;
                        m_audioSource.PlayOneShot(as_duplicate);
                        m_animator.SetTrigger("Duplicate");
                        Instantiate(duplicateParticules, transform.position, Quaternion.identity);
                    }
                    else
                    {
                        currentState = RedBallState.Click;
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
                    currentState = RedBallState.Drag;
                    isDragged = true;
                }
                break;

            case RedBallState.Click:
                currentState = RedBallState.Idle;
                break;

            case RedBallState.Duplicate:
                currentState = RedBallState.Idle;
                break;

            case RedBallState.Drag:
                if (Input.GetMouseButton(1))
                {
                    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    transform.position = mouseWorldPos + (Vector2)dragOffset;
                    m_rb.velocity = Vector2.zero;
                }
                if (Input.GetMouseButtonUp(1) || isInhaled)
                {
                    currentState = RedBallState.Idle;
                    GameManager.Instance.isDragging = false;
                    isDragged = false;
                }
                break;

            case RedBallState.Inhale:
                m_rb.velocity = Vector2.zero;
                break;
        }
    }

    // Méthode de vérification personnalisée pour déterminer si la souris est sur l'objet
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

    private IEnumerator SpawnProp()
    {
        GameObject newObject = Instantiate(gameObject, transform.position, Quaternion.identity);
        newObject.name = gameObject.name;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Debug.Log(randomDir);
        newObject.GetComponent<Rigidbody2D>().AddForce(randomDir * force, ForceMode2D.Impulse);
        yield return null;
    }
}
