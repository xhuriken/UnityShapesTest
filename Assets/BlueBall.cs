using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class BlueBall : MonoBehaviour
{
    [Header("Oscillation Settings")]
    public float amplitude = 5f; // Distance maximale par rapport � l'origine sur l'axe Y
    public float speed = 2f;     // Vitesse de d�placement verticale

    [Header("Friction Settings")]
    public float frictionFactor = 0.99f;      // Facteur de friction
    public float frictionThreshold = 0.01f;   // Seuil pour arr�ter la friction

    // Position d'origine et bornes d'oscillation
    private float originY;
    private float topY;
    private float bottomY;

    // Machine � �tats
    private enum BallState { Oscillating, Friction, Duplicate }
    private BallState currentState = BallState.Oscillating;

    private Rigidbody2D rb;
    // Direction verticale : 1 = vers le haut, -1 = vers le bas
    private int direction = 1;

    // R�f�rence au script Prop et stockage de l'�tat pr�c�dent de drag
    private Prop prop;
    private bool wasDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        prop = GetComponent<Prop>();

        // Initialisation de l'origine et des bornes d'oscillation
        ResetOrigin(transform.position);

        // D�marrer le mouvement vers le haut
        rb.velocity = new Vector2(0, speed * direction);
    }

    void Update()
    {
        // V�rifier l'�tat de drag via le script Prop
        if (prop != null)
        {
            // Si on vient de terminer un drag (passage de vrai � faux)
            if (wasDragging && !prop.isDragged)
            {
               
                // R�initialiser l'origine de l'oscillation � la position actuelle
                ResetOrigin(transform.position);
                currentState = BallState.Oscillating;
                rb.velocity = new Vector2(0, speed * direction);
            }
            wasDragging = prop.isDragged;
        }

        // Machine � �tats pour le mouvement de la balle
        switch (currentState)
        {
            case BallState.Oscillating:
                // V�rifier si la balle a atteint une des limites et inverser la direction
                if (direction > 0 && transform.position.y >= topY)
                {
                    direction = -1;
                    rb.velocity = new Vector2(0, speed * direction);
                }
                else if (direction < 0 && transform.position.y <= bottomY)
                {
                    direction = 1;
                    rb.velocity = new Vector2(0, speed * direction);
                }
                break;

            case BallState.Friction:
                // Appliquer la friction sur la v�locit�
                rb.velocity *= frictionFactor;
                // Lorsque la vitesse devient trop faible, reprendre l'oscillation
                if (rb.velocity.magnitude < frictionThreshold)
                {
                    rb.velocity = Vector2.zero;
                    // Choisir la direction en fonction de la position actuelle
                    float distToTop = Mathf.Abs(topY - transform.position.y);
                    float distToBottom = Mathf.Abs(transform.position.y - bottomY);
                    direction = (distToTop < distToBottom) ? -1 : 1;
                    rb.velocity = new Vector2(0, speed * direction);
                    currentState = BallState.Oscillating;
                }
                break;

            case BallState.Duplicate:



                rb.velocity = Vector2.zero;
                StartCoroutine(changeState());

                break;

        }
                if (prop.currentState == Prop.PropState.Duplicate)
                {
                    currentState = BallState.Duplicate;
                }
    }
    // Lorsqu'une collision avec un objet tagu� "Ball" est d�tect�e, passage en mode friction
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {

                currentState = BallState.Friction;
        }
    }

    /// <summary>
    /// Met � jour l'origine de l'oscillation en fonction d'une nouvelle position.
    /// </summary>
    /// <param name="newOrigin">La nouvelle position (utilise uniquement l'axe Y)</param>
    public void ResetOrigin(Vector3 newOrigin)
    {
        originY = newOrigin.y;
        topY = originY + amplitude;
        bottomY = originY - amplitude;
    }

    private IEnumerator changeState()
    {
       yield return new WaitForSeconds(2f);
       currentState = BallState.Oscillating;
    }
}
