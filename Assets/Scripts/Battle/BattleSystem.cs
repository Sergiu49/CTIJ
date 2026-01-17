using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState{ Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    
    [SerializeField] BattleDialog dialogbox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    private BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;
    
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
        
        
        partyScreen.Init();
        
        dialogbox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogbox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared");
        ChooseFirstTurn();
    }

   //BattleOver schimba starea la BattleOver, iar functia OnBattleOver este un event care anunta finalul luptei
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(dialogbox.TypeDialog("Choose an action"));
        dialogbox.EnableActionSelector(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartyScreenSelection();
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
                MoveSelection();
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
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void HandlePartyScreenSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;
        
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        
        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send a fainted pokemon!");
                return;
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same pokemon!");
                return;
            }
            
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        if(playerUnit.Pokemon.HP > 0)
        {
            currentPokemonFainted = false;
            yield return dialogbox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayDeadAnimation();
            yield return new WaitForSeconds(1f);
        }
        
        playerUnit.Setup(newPokemon);
        dialogbox.SetMoveNames(newPokemon.Moves);
        yield return dialogbox.TypeDialog($"GO {newPokemon.Base.Name}!");

        if (currentPokemonFainted)
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove());
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
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            dialogbox.EnableMoveSelector(false);
            dialogbox.EnableDialogText(true);
            ActionSelection();
        }
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit,  move);
       
        //Daca nu s-a schimbat starea de catre RunMove, atunci incepe EnemyMove
        if(state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);
        
        //Daca nu s-a schimbat starea de catre RunMove, atunci incepe EnemyMove
        if (state == BattleState.PerformMove)
            ActionSelection();
        
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogbox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");
        
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        targetUnit.PlayHurtAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
           yield return RunMoveEffects(move, sourceUnit.Pokemon,  targetUnit.Pokemon);
        }
        else
        {
            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }
        
        if (targetUnit.Pokemon.HP <= 0)
        {
            yield return dialogbox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted");
            targetUnit.PlayDeadAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }
        
        //Unele statusuri pot rani pokemonul asa ca trebuie sa verificam daca mai sunt in viata
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogbox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted");
            sourceUnit.PlayDeadAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        var effects = move.Base.Effect;
        
        //Stat Boost
        if (effects.Boosts != null)
        {
            if(move.Base.Target==MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //Status Condition
        if (effects.Status != ConditionsID.none)
        {
            target.SetStatus(effects.Status);
        }
        
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }
    IEnumerator ShowStatusChanges(Pokemon  pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogbox.TypeDialog(message);
        }
    }

    private void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.isPlayerUnint)
        {
            var nextPokemon = playerParty.GetHealtyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            BattleOver(true);
        }
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
            ActionSelection();
        else
            StartCoroutine(EnemyMove());
    }
}
