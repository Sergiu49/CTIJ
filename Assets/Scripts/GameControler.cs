using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState{ FreeRoam, Battle, Dialog, Menu, Cutscene, Paused}
public class GameControler : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    
    GameState state;
    GameState stateBeforePause;
    
    public static GameControler Instance { get; private set; }

    MenuController menuController;
    
    private void Awake()
    {
        ConditionsDB.Init();
        Instance = this;
        
        menuController = GetComponent<MenuController>();
        
    }
    
    public void Start()
    {
        
        battleSystem.OnBattleOver += EndBattle;
        
        DialogManager.Instance.OnShowDialog += () =>
        {

            state = GameState.Dialog;

        };
        
        DialogManager.Instance.OnCloseDialog += () =>
        {
            
            if(state == GameState.Dialog)
            state = GameState.FreeRoam;

        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected; //normal function

    }

    public void PauseGame(bool pause)
    {

        if (pause)
        {
            
            stateBeforePause = state;
            state = GameState.Paused;
            
        }
        else
        {
           state=stateBeforePause;
        }
        
    }

    private void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindAnyObjectByType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);


        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }
    
    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();
        
        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainerView(TrainerController trainer)
    {
        
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
        
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                
                menuController.OpenMenu();
                state=GameState.Menu;
                
            }
            
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            
            DialogManager.Instance.HandleUpdate();
            
        }
        else if (state == GameState.Menu)
        {

            menuController.HandleUpdate();

        }
    }

    void OnMenuSelected(int selectedItem)
    {

        if (selectedItem == 0)
        {
            
            //Pokemon
        }

        state = GameState.FreeRoam;

    }
    
}
