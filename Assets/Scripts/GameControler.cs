using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState{ FreeRoam, Battle, Dialog, Cutscene}
public class GameControler : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    
    GameState state;

    public void Start()
    {
        playerController.onEncounter += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        playerController.OnEnterTrainerView += (Collider2D trainerCollider) =>
        {
            var trainer=trainerCollider.GetComponentInParent<TrainerController>(); //the collider for fov is in child object FOV
            if (trainer != null)
            {

                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));

            }
        };

    DialogManager.Instance.OnShowDialog += () =>
        {

            state = GameState.Dialog;

        };
        
        DialogManager.Instance.OnCloseDialog += () =>
        {
            
            if(state == GameState.Dialog)
            state = GameState.FreeRoam;

        };
        
    }

    private void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        
        battleSystem.StartBattle();
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            
            DialogManager.Instance.HandleUpdate();
            
        }
    }
}
