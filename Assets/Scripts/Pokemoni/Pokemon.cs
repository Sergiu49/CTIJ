using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    public PokemonBase _base { get; set; }
    public int level { get; set; }

    // These properties are created in Video #6 to store dynamic battle data
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    // Constructor: Called when creating a new Pokemon (e.g., encountering a wild one)
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        // Initialize HP to the maximum calculated HP
        HP = MaxHP;

        // Generate Moves based on Level (Logic from Video #6)
        Moves = new List<Move>();
        foreach (var move in _base.LearnableMoves)
        {
            // Only learn moves if the level requirement is met
            if (move.Level <= level)
            {
                Moves.Add(new Move(move.Base));
            }

            // Pokemon can only hold 4 moves
            if (Moves.Count >= 4)
                break;
        }
    }

    // Properties to expose private fields safely
    public PokemonBase Base {
        get { return _base; }
    }

    public int Level {
        get { return level; }
    }

    // Stat Calculations (Logic from Video #5)
    // Formula: (Base * Level) / 100 + 5
    public int Attack {
        get { return Mathf.FloorToInt((_base.Attack * level) / 100f) + 5; }
    }

    public int Defense {
        get { return Mathf.FloorToInt((_base.Defense * level) / 100f) + 5; }
    }

    public int SpAttack {
        get { return Mathf.FloorToInt((_base.SpAttack * level) / 100f) + 5; }
    }

    public int SpDefense {
        get { return Mathf.FloorToInt((_base.SpDefense * level) / 100f) + 5; }
    }

    public int Speed {
        get { return Mathf.FloorToInt((_base.Speed * level) / 100f) + 5; }
    }

    // MaxHP uses a slightly different formula (+10 instead of +5)
    public int MaxHP {
        get { return Mathf.FloorToInt((_base.MaxHP * level) / 100f) + 10; }
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100 <= 6.25)
            critical = 2f;
        
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        Debug.Log(type);
        
        var damageDetails = new DamageDetails()
        {
            TypeEffectivness = type,
            Critical = critical,
            Fainted = false
        };
        
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        
        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }
        return damageDetails;
    }

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectivness { get; set; }
}