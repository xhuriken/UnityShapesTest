using Shapes;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

public class Prop : MonoBehaviour
{

    //Ca c'est un Prop, object commun entre touts les forme
    //Ce script sert a donnée les characteristique de la forme

    [SerializeField]
    private int duplicateCount = 3;
    private int clickCount = 0;
    private Animator m_animator;

    public float force = 2f;
    public GameObject duplicateParticules;
    private bool isDragging = false;
    private CircleCollider2D m_cc;
    private AudioSource m_audioSource;
    public AudioClip as_duplicate;
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_cc = GetComponent<CircleCollider2D>();
        m_audioSource = GetComponent<AudioSource>();
    }

    private bool isMouseOver = false;

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {

        if (!GameManager.Instance.isDragging)
        {
            isMouseOver = false;
        }
    }

    private void Update()
    {
        if (isMouseOver && Input.GetMouseButton(1))
        {
            if (!GameManager.Instance.isDragging)
            {
                GameManager.Instance.isDragging = true;
                isDragging = true;
            }

            if (isDragging)
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position = mousePos;
            }
        }

        if (GameManager.Instance.isDragging && Input.GetMouseButtonUp(1))
        {
            GameManager.Instance.isDragging = false;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Si la mousPos est dans le cercle
            if (Vector2.Distance(mousePos, transform.position) < m_cc.radius)
            {
                isMouseOver = true;
            }
        }
    }
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickCount++;

            if (clickCount >= duplicateCount)
            {
                clickCount = 0;
                m_audioSource.PlayOneShot(as_duplicate);
                m_animator.SetTrigger("Duplicate");
                Instantiate(duplicateParticules, transform.position, Quaternion.identity);
            }
            else
            {
                m_animator.SetTrigger("Click");
            }
        }


    }

    private IEnumerator SpawnProp()
    {

        GameObject newObject = Instantiate(gameObject, transform.position, Quaternion.identity);
        newObject.name = gameObject.name;
        Vector2 randomDir = Random.insideUnitCircle.normalized; 
        newObject.GetComponent<Rigidbody2D>().AddForce(randomDir * force, ForceMode2D.Impulse);

        yield return null;

    }

}
