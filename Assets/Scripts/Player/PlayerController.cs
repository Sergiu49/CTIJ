using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask grassLayer; 

    private bool isMoving;
    private Vector2 input;

   
    

    private void Update()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal"); //folisim raw pt a lua valori +1 -1 sau 0 sa ne fie mai simplu
            input.y = Input.GetAxisRaw("Vertical");

           
            if (input.x != 0) // nu lasa sa mergi diagonal
            input.y = 0;

            if (input != Vector2.zero) //verificam daca vectorul input sa schimbat
            {
                

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        
       
    }

    private bool IsWalkable(Vector3 targetPos) // verificam daca locul un ar ajunge playerul se suprapune cu planul solid objects
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