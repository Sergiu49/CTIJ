using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    
    public event Action onEncounter;
    
    private Vector2 input;
    
   private Character character;

   private void Awake()
   {
       character=GetComponent<Character>();
   }


   public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // Prevent diagonal movement
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                
               StartCoroutine(character.Move(input, CheckForEncounters));
                
            }
        }
        
        character.HandleUpdate();
        
       if(Input.GetKeyDown(KeyCode.Z))
           Interact();
       
    }

    void Interact()
    {

        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);

        }
    }
    
    private void CheckForEncounters()
    {
        //Verificare pozitie curenta in iarba
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.GrassLayer) != null)
        {
            // 10% sanse ca sa intalnesti un Pokemon
            if (Random.Range(1, 101) <= 10)
            {
                Debug.Log("Encountered a wild Pokemon!");
                character.Animator.IsMoving = false;
                onEncounter();
            }
        }
    }
}