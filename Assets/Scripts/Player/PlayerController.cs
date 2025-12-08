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

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;

        //Verificare pentru interactiuni
        CheckForEncounters();
    }

    private void CheckForEncounters()
    {
        //Verificare pozitie curenta in iarba
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            // 10% sanse ca sa intalnesti un Pokemon
            if (Random.Range(1, 101) <= 10)
            {
                Debug.Log("Encountered a wild Pokemon!");
            }
        }
    }
}