using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class BlueBall : MonoBehaviour
{
    [Header("Oscillation Settings")]
    public float amplitude = 5f; // Distance maximale par rapport à l'origine sur l'axe Y
    public float speed = 2f;     // Vitesse de déplacement verticale

    [Header("Friction Settings")]
    public float frictionFactor = 0.99f;      // Facteur de friction
    public float frictionThreshold = 0.01f;   // Seuil pour arrêter la friction

    // Position d'origine et bornes d'oscillation
    private float originY;
    private float topY;
    private float bottomY;

    // Machine à états
    private enum BallState { Oscillating, Friction, Duplicate }
    private BallState currentState = BallState.Oscillating;

    private Rigidbody2D rb;
    // Direction verticale : 1 = vers le haut, -1 = vers le bas
    private int direction = 1;

    // Référence au script Prop et stockage de l'état précédent de drag
    private Prop prop;
    private bool wasDragging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        prop = GetComponent<Prop>();

        // Initialisation de l'origine et des bornes d'oscillation
        ResetOrigin(transform.position);

        // Démarrer le mouvement vers le haut
        rb.velocity = new Vector2(0, speed * direction);
    }

    void Update()
    {
        // Vérifier l'état de drag via le script Prop
        if (prop != null)
        {
            // Si on vient de terminer un drag (passage de vrai à faux)
            if (wasDragging && !prop.isDragged)
            {
               
                // Réinitialiser l'origine de l'oscillation à la position actuelle
                ResetOrigin(transform.position);
                currentState = BallState.Oscillating;
                rb.velocity = new Vector2(0, speed * direction);
            }
            wasDragging = prop.isDragged;
        }

        // Machine à états pour le mouvement de la balle
        switch (currentState)
        {
            case BallState.Oscillating:
                // Vérifier si la balle a atteint une des limites et inverser la direction
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
                // Appliquer la friction sur la vélocité
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
    // Lorsqu'une collision avec un objet tagué "Ball" est détectée, passage en mode friction
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {

                currentState = BallState.Friction;
        }
    }

    /// <summary>
    /// Met à jour l'origine de l'oscillation en fonction d'une nouvelle position.
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
