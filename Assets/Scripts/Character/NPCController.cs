using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{

    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2>movementPattern;
    [SerializeField] float timeBetweenPattern;
    
    Character character;

    private float idleTimer=0f;
    private NPCState state;
    private int currentPattern=0;
    
    private void Awake()
    {
        character=GetComponent<Character>();
    }
    
    public void Interact(Transform initiator)
    {
        
        if(state== NPCState.Idle)
        {
        
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, ()=>
            {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));

        }
        
    }

    private void Update()
    {
        
        if(state== NPCState.Idle)
        {
            idleTimer+=Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {

                idleTimer = 0f;
                if(movementPattern.Count>0)
                StartCoroutine(Walk()); 
                
            }
                
        }
        character.HandleUpdate();
        
    }

    IEnumerator Walk()
    {
        
        state = NPCState.Walking;
        
        var oldPos = transform.position;
        
        yield return character.Move(movementPattern[currentPattern]); //coroutine
        
        if(transform.position!=oldPos) //the character walked in the previous pattern
        currentPattern = (currentPattern + 1) % movementPattern.Count;
        
        state = NPCState.Idle;
        
    }
    
    
}
public enum NPCState {Idle, Walking, Dialog }
