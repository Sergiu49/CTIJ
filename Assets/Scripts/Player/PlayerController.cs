using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask grassLayer; 

    private bool isMoving;
    private Vector2 input;

   private Animator animator;

   private void Awake()
   {
       animator=GetComponent<Animator>();
   }


   private void Update()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // Prevent diagonal movement
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        
       animator.SetBool("isMoving", isMoving);
        
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }
        return true;
    }

    IEnumerator Move(Vector3 targetPos) //folosim corutina ca de genu ca ea mere mai multe frame-uri si o sa avem efect de miscare nu o sa fie teleportare 
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

       
        CheckForEncounters();
    }

    private void CheckForEncounters() //aici verificam daca playerul se afla in layerul longgrass si daca da generam un nr aleatoriu pe baza caruia da encounter 
    {
       
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
          
            if (Random.Range(1, 101) <= 10)
            {
                Debug.Log("Encountered a wild Pokemon!");
            }
        }
    }
}