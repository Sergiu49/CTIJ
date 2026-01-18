using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kyp in Conditions)
        {
            var conditionId = kyp.Key;
            var condition = kyp.Value;
            
            condition.Id = conditionId;
        }
    }
    
    public static Dictionary<ConditionsID, Conditions> Conditions { get; set; } = new Dictionary<ConditionsID, Conditions>()
    {
        {
            ConditionsID.psn,
            new Conditions()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by poison");
                }
            }
        },
        {
            ConditionsID.brn,
            new Conditions()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHP / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by burn");
                }
            }
        },
        {
            ConditionsID.par,
            new Conditions()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralyzed and can't move");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionsID.frz,
            new Conditions()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s not frozen anymore");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditionsID.slp,
            new Conditions()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Pokemon pokemon) =>
                {
                    // Sleep for 1-3 turns
                    pokemon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} moves");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping");
                    return false;
                }
            }
        },
            //Volatile status
            {
                ConditionsID.confusion,
                new Conditions()
                {
                    Name = "Confusion",
                    StartMessage = "has been confused",
                    OnStart = (Pokemon pokemon) =>
                    {
                        // Confused for 1 - 4 turns
                        pokemon.VolatileStatusTime = Random.Range(1, 5);
                        Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} moves");
                    },
                    OnBeforeMove = (Pokemon pokemon) =>
                    {
                        if (pokemon.VolatileStatusTime <= 0)
                        {
                            pokemon.CureVolatileStatus();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} kicked out of confusion!");
                            return true;
                        }
                        pokemon.VolatileStatusTime--;

                        // 50% chance to do a move
                        if (Random.Range(1, 3) == 1)
                            return true;

                        // Hurt by confusion
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused");
                        pokemon.UpdateHP(pokemon.MaxHP / 8);
                        pokemon.StatusChanges.Enqueue($"It hurt itself due to confusion");
                        return false;
                    }
                }
            }
        
    };

   public static float GetStatusBonus(Conditions condition)
{
    if (condition == null)
        return 1f;

    
    else if (condition.Id == ConditionsID.slp || condition.Id == ConditionsID.frz)
        return 2f;
    else if (condition.Id == ConditionsID.par || condition.Id == ConditionsID.psn || condition.Id == ConditionsID.brn)
        return 1.5f;
    
    return 1f;
}
}

public enum ConditionsID
{
    none, psn, brn, slp, par, frz, 
    confusion
}
