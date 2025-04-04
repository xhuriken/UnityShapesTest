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
    //public float ct_click = 0.1f;
    //public float ct_duplicate = 0.6f;
    //private float currentct_click = 0f;
    //private float currentct_duplicate = 0f;
    // Start is called before the first frame update
    private bool isDragging = false;
    private CircleCollider2D m_cc;
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_cc = GetComponent<CircleCollider2D>();
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
                //Debug.Log("Click count reset to 0");
                m_animator.SetTrigger("Duplicate");
                //currentct_duplicate = 0f;

            }
            else
            {
                m_animator.SetTrigger("Click");
                //currentct_click = 0f;
            }
        }


    }

    private IEnumerator SpawnProp()
    {

        GameObject newObject = Instantiate(gameObject, transform.position, Quaternion.identity);
        //Debug.Log("New object spawned: " + newObject.name);

        //Disc newDisc = newObject.GetComponent<Disc>();
        //WaitForSeconds wait = new WaitForSeconds(0.1f);
        Vector2 randomDir = Random.insideUnitCircle.normalized; 
        newObject.GetComponent<Rigidbody2D>().AddForce(randomDir * force, ForceMode2D.Impulse);
        //newObject.GetComponent<CircleCollider2D>().enabled = true;
        //float elapsed = 0f;
        //float startRadius = 0;
        //float targetRadius = startRadius + newDisc.Radius;

        //while (elapsed < 0.3f)
        //{
        //    elapsed += Time.deltaTime;
        //    float newRadius = Mathf.Lerp(startRadius, targetRadius, elapsed / 0.3f);
        //    newDisc.Radius = newRadius;
        //    Debug.Log("Elapsed time: " + elapsed);
        //    Debug.Log("newRadius : " + newRadius);
        //    yield return null;
        //}

        //newDisc.Radius = targetRadius;
        yield return null;

    }


}
