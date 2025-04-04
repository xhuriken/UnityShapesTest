using System.Collections;
using UnityEngine;

public class BlueBall : MonoBehaviour
{
    [Header("Oscillation Settings")]
    // Amplitude de l'oscillation (distance maximale par rapport � la position d'origine)
    public float amplitude = 5f;
    // Vitesse de d�placement (valeur absolue de la v�locit�)
    public float speed = 2f;

    [Header("Friction Settings")]
    // Facteur de r�duction de la v�locit� lorsqu'en �tat Friction
    public float frictionFactor = 0.99f;
    // Seuil sous lequel la v�locit� est consid�r�e comme nulle
    public float frictionThreshold = 0.01f;

    // Position d'origine sur l'axe Y
    private float originY;
    // Limites calcul�es par rapport � l'origine
    private float topY;
    private float bottomY;

    // Machine � �tats pour le mouvement
    private enum OscillationState { MovingUp, MovingDown, Friction }
    private OscillationState currentState;

    private Rigidbody2D m_rb;
    private bool isCoroutineRunning = false;

    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();

        // Stocker la position d'origine et calculer les limites d'oscillation
        originY = transform.position.y;
        topY = originY + amplitude;
        bottomY = originY - amplitude;

        // D�finir l'�tat initial en fonction de la position actuelle par rapport � l'origine
        currentState = (transform.position.y <= originY) ? OscillationState.MovingUp : OscillationState.MovingDown;
        SetVelocityForState();
    }

    void Update()
    {
        switch (currentState)
        {
            case OscillationState.MovingUp:
                if (transform.position.y >= topY)
                {
                    transform.position = new Vector3(transform.position.x, topY, transform.position.z);
                    currentState = OscillationState.MovingDown;
                    SetVelocityForState();
                }
                break;

            case OscillationState.MovingDown:
                if (transform.position.y <= bottomY)
                {
                    transform.position = new Vector3(transform.position.x, bottomY, transform.position.z);
                    currentState = OscillationState.MovingUp;
                    SetVelocityForState();
                }
                break;

            case OscillationState.Friction:
                // Appliquer la friction sur la v�locit�
                m_rb.velocity *= frictionFactor;
                if (m_rb.velocity.magnitude < frictionThreshold)
                {
                    m_rb.velocity = Vector2.zero;
                    // Une fois stopp�e, reprendre l'oscillation en fonction de la position par rapport � l'origine
                    currentState = (transform.position.y <= originY) ? OscillationState.MovingUp : OscillationState.MovingDown;
                    SetVelocityForState();
                }
                break;
        }

        // Si la v�locit� est nulle et aucune coroutine n'est en cours, lancer la coroutine pour remettre � jour l'origine
        if (m_rb.velocity == Vector2.zero && !isCoroutineRunning)
        {
            StartCoroutine(WaitForOscillation());
        }
    }

    // Affecte la v�locit� en fonction de l'�tat courant (en dehors de Friction)
    private void SetVelocityForState()
    {
        if (currentState == OscillationState.MovingUp)
            m_rb.velocity = new Vector2(0, speed);
        else if (currentState == OscillationState.MovingDown)
            m_rb.velocity = new Vector2(0, -speed);
    }

    // Lorsqu'une collision avec un objet tagu� "Ball" est d�tect�e, passer en �tat Friction
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            currentState = OscillationState.Friction;
        }
    }

    // Coroutine qui remet � jour l'origine et les limites, puis red�marre l'oscillation
    private IEnumerator WaitForOscillation()
    {
        Debug.Log("newpos");
        isCoroutineRunning = true;
        // Mettre � jour l'origine � la position actuelle
        originY = transform.position.y;
        topY = originY + amplitude;
        bottomY = originY - amplitude;
        Debug.Log(originY);
        Debug.Log(topY);
        Debug.Log(bottomY);

        // Attendre un court instant pour stabiliser la position
        yield return new WaitForSeconds(1f);

        // On choisit ici de reprendre en descendant (vous pouvez adapter selon votre logique)
        currentState = (OscillationState.MovingDown);
        SetVelocityForState();

        // Attendre encore un peu avant de lib�rer la coroutine

        isCoroutineRunning = false;
    }
}
