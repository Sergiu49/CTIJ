using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState{ Start, PlayerAction, PlayerMove, EnemyMove, Busy}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialog dialogbox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    private BattleState state;
    int currentAction;
    int currentMove;
    
    PokemonParty playerParty;
    Pokemon wildPokemon;
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealtyPokemon());
        enemyUnit.Setup(wildPokemon);
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);
        
        partyScreen.Init();
        
        dialogbox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogbox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared");
        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogbox.TypeDialog("Choose an action"));
        dialogbox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogbox.EnableActionSelector(false);
        dialogbox.EnableDialogText(false);
        dialogbox.EnableMoveSelector(true);
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical>1f)
            yield return dialogbox.TypeDialog("A critical hit!");
        
        if (damageDetails.TypeEffectivness > 1f)
            yield return dialogbox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectivness < 1f)
            yield return dialogbox.TypeDialog("It's not very effective...");
        
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;
        
        currentAction = Mathf.Clamp(currentAction, 0, 3);
        
        dialogbox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentAction == 0)
            {
                // Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                // Bag
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
            }
        }
    }

    void OpenPartyScreen()
    {
        //print("Opening Party Screen");
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }
        
        dialogbox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            dialogbox.EnableMoveSelector(false);
            dialogbox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            dialogbox.EnableMoveSelector(false);
            dialogbox.EnableDialogText(true);
            PlayerAction();
        }
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        
        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PP--;
        yield return dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");
        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        enemyUnit.PlayHurtAnimation();
        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
        
        if (damageDetails.Fainted)
        {
            yield return dialogbox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Fainted");
            enemyUnit.PlayDeadAnimation();
            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--;
        yield return dialogbox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        playerUnit.PlayHurtAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted");
            playerUnit.PlayDeadAnimation();
            yield return new WaitForSeconds(2f);

            var nextPokemon = playerParty.GetHealtyPokemon();
            if (nextPokemon != null)
            {
                playerUnit.Setup(nextPokemon);
                playerHud.SetData(nextPokemon);

                dialogbox.SetMoveNames(nextPokemon.Moves);

                yield return dialogbox.TypeDialog($"Go {nextPokemon.Base.Name}!");

                PlayerAction();
            }
            else
            {
                OnBattleOver(false);
            }
        }
        else
        {
            PlayerAction();
        }
    }
}
