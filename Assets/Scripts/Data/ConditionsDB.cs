using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    
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
        }
    };
}

public enum ConditionsID
{
    none, psn, brn, slp, par, frz
}
