using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    
    CharacterAnimation animator;
    public float moveSpeed;
    
    public bool IsMoving { get; private set; }

    public float OffsetY { get; private set; } = 0.3f;

    private void Awake()
    {
        
        animator = GetComponent<CharacterAnimation>();
        SetPositionAndSnapToTile(transform.position);
        
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f +OffsetY;
        
        transform.position = pos;

    }
    
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null)
    {
        
        animator.MoveX = Mathf.Clamp(moveVec.x,-1f,1f);
        animator.MoveY = Mathf.Clamp(moveVec.y,-1f,1f);
                
        var targetPos = transform.position; 
        targetPos.x += moveVec.x; //calculeaza unde vrea sa ajunga
        targetPos.y += moveVec.y;

        if (!IsPathClear(targetPos)) // verifica daca poate sa ajunga acolo
            yield break;
        
        IsMoving = true; // ii aici ca sa nu lase sa  sa faca alte actiuni

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon) //verifica daca a ajuns la destintatie folosind o functie fancy
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime); // il muta per se
            yield return null;
        }

        transform.position = targetPos; // il opreste la destinatie
        IsMoving = false; // scoate conditia de moving si se poate sa se miste 

        OnMoveOver?.Invoke(); // verifica daca se intampla ceva gen tall grass, perete
        
    }
    
    public void HandleUpdate()
    {
     
        animator.IsMoving=IsMoving;
        
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        
        var diff=targetPos - transform.position;
        var dir=diff.normalized;
        if(Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f,dir,diff.magnitude -1, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer)==true)
        return false; //not clear
        
        return true; //walkable


    }
    
    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    public void LookTowards(Vector3 targetPos)
    {
        
        var xdiff=Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x); //diferenta ca si intreg si nu zecimal
        var ydiff=Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            
            animator.MoveX = Mathf.Clamp(xdiff,-1f,1f);
            animator.MoveY = Mathf.Clamp(ydiff,-1f,1f);
            
        }
        else
        {
            Debug.LogError("Error in look towards: you can't look diagonally");
        }
        
    }

    //proprietate
    public CharacterAnimation Animator
    {
        get => animator;
    }
}
