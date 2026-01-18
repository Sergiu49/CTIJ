using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BattleState{ Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, MoveForget, BattleOver}
public enum BattleAction{Move, SwitchPokemon, UseItem, Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    
    [SerializeField] BattleDialog dialogbox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    
    PokemonParty playerParty;
    Pokemon wildPokemon;
    
    MoveBase moveToLearn;
    
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
        ActionSelection();
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
        else if (state == BattleState.MoveForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == 4)
                {
                    //nu invata noua miscare
                    StartCoroutine(dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn}"));
                }
                else
                {
                    //uita miscarea si invata una noua
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;

                    StartCoroutine(dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                    
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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
                prevState = state;
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
            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if(playerUnit.Pokemon.HP > 0)
        {
            yield return dialogbox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayDeadAnimation();
            yield return new WaitForSeconds(1f);
        }
        
        playerUnit.Setup(newPokemon);
        dialogbox.SetMoveNames(newPokemon.Moves);
        yield return dialogbox.TypeDialog($"GO {newPokemon.Base.Name}!");

        state = BattleState.RunningTurn;
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
            var move = playerUnit.Pokemon.Moves[currentMove];
            if(move.PP == 0) return;
            
            dialogbox.EnableMoveSelector(false);
            dialogbox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            dialogbox.EnableMoveSelector(false);
            dialogbox.EnableDialogText(true);
            ActionSelection();
        }
    }


    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newmove)
    {
        state = BattleState.Busy;
        yield return dialogbox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(),  newmove);
        moveToLearn = newmove;

        state = BattleState.MoveForget;
    }
    
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();
            
            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            
            //Check who goes first
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }
            
            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ?  enemyUnit  : playerUnit;
            
            var secondPokemon = secondUnit.Pokemon;

            //First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                //Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            
            //Enemy turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        

        
        move.PP--;
        yield return dialogbox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHurtAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effect, sourceUnit.Pokemon,  targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondaries in move.Base.Secondaries)
                {
                    var rnd = Random.Range(1, 101);
                    if (rnd <= secondaries.Chance)
                    {
                        yield return RunMoveEffects(secondaries, sourceUnit.Pokemon,  targetUnit.Pokemon, secondaries.Target);
                    }
                }
            }
        
            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogbox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s move missed");
        }
        
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget  moveTarget)
    {
        //Stat Boost
        if (effects.Boosts != null)
        {
            if(moveTarget==MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //Status Condition
        if (effects.Status != ConditionsID.none)
        {
            target.SetStatus(effects.Status);
        }
        
        //Volatile Status Condition
        if (effects.VolatileStatus != ConditionsID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }
        
        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        
        //Unele statusuri pot rani pokemonul asa ca trebuie sa verificam daca mai sunt in viata
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }    
    }
    
    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogbox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} Fainted");
        faintedUnit.PlayDeadAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.isPlayerUnint)
        {
            //exp gain
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
           
            //float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;
            // int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus) / 7;
           
            int expGain = Mathf.FloorToInt((expYield * enemyLevel) / 7);

            playerUnit.Pokemon.Exp += expGain;
            yield return dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            //check lvl up
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}");
                
                //Try to learn new move
                var newmove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newmove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < 4)
                    {
                        playerUnit.Pokemon.Learnmove(newmove);
                        yield return dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newmove.Base.Name}");
                        dialogbox.SetMoveNames(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        //forget old move
                        yield return dialogbox.TypeDialog($"{playerUnit.Pokemon.Base.Name} is trying to learn {newmove.Base.Name}");
                        yield return dialogbox.TypeDialog($"But it cannot learn more then 4 moves");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newmove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveForget);
                        yield return new WaitForSeconds(2f);
                    }
                }
                
                yield return playerUnit.Hud.SetExpSmooth(true);
            }

        }

        CheckForBattleOver(faintedUnit);
    }
    
    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if(move.Base.AlwaysHits)
            return true;
        
        float moveAccuracy = move.Base.Accuracy;
        
        int accuracy = source.StatsBoosts[Stat.Accuracy];
        int evasion = source.StatsBoosts[Stat.Evasion];
        
        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
    
}
