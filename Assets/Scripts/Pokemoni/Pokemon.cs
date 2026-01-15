using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

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
}