using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Pokemon
{
    [SerializeField]  PokemonBase _base;
    [SerializeField]  int _level;


    public Pokemon(PokemonBase pBase, int pLevel)
{
    _base = pBase;
    level = pLevel;

    init();
}
     
    public PokemonBase @base
    {
        get{return _base;}
    }
    public int level { 
    get { return _level; } 
    set { _level = value; } 
}

    // These properties are created in Video #6 to store dynamic battle data
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    
    public Move CurrentMove { get; set; }
    
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatsBoosts { get; private set; }
    public Conditions Status {get; private set;}
    public int StatusTime {get; set;}
    
    public Conditions VolatileStatus {get; private set;}
    public int VolatileStatusTime {get; set;}

    
    public Queue<string> StatusChanges { get; private set; } 
    public bool HpChanged {get; set;}
    public event Action OnStatusChange;

    // Constructor: Called when creating a new Pokemon (e.g., encountering a wild one)
    public void init()
    {

        // Generate Moves based on Level (Logic from Video #6)
        Moves = new List<Move>();
        foreach (var move in @base.LearnableMoves)
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
        
        CalculateStats();
        
        // Initialize HP to the maximum calculated HP
        HP = MaxHP;

        StatusChanges = new Queue<string>();
        ResetStatsBoosts();
        Status = null;
        VolatileStatus = null;
    }


    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((@base.Attack * level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((@base.Defense * level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((@base.SpAttack * level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((@base.SpDefense * level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((@base.Speed * level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((@base.MaxHP * level) / 100f) + 10 + level;
    }

    void ResetStatsBoosts()
    {
        StatsBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0},
            { Stat.Defense, 0},
            { Stat.SpAttack , 0},
            { Stat.SpDefense , 0},
            { Stat.Speed, 0},
            { Stat.Accuracy, 0},
            { Stat.Evasion, 0},
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        
        //Stats boost
        int boost=StatsBoosts[stat];
        var boostsValue = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostsValue[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostsValue[-boost]);
        }
        
        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatsBoosts[stat] = Mathf.Clamp(StatsBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
            
            Debug.Log($"{stat} has been boosted to {StatsBoosts[stat]}");
        }
    }

    public void SetStatus(ConditionsID conditionId)
    {
        if(Status!=null) return;
        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}'s {Status.StartMessage}");
        OnStatusChange?.Invoke();
    }
    
    public void SetVolatileStatus(ConditionsID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChange?.Invoke();
    }
    
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HpChanged = true;
    }

    // Properties to expose private fields safely
    public PokemonBase Base {
        get { return @base; }
    }

    public int Level {
        get { return level; }
    }

    // Stat Calculations (Logic from Video #5)
    // Formula: (Base * Level) / 100 + 5
    public int Attack {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed {
        get { return GetStat(Stat.Speed); }
    }

    // MaxHP uses a slightly different formula (+10 instead of +5)
    public int MaxHP { get; private set; }
    

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

        float attack = (move.Base.Category == MoveCategory.Special) ? (float)attacker.SpAttack : (float)attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? (float)SpDefense : (float)Defense;
        
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        
        
        UpdateHP(damage);
        
        return damageDetails;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x=> x.PP > 0).ToList();
        
        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }

        return canPerformMove;
    }
    public void OnAfterTurn()
    {
        if (Status != null && Status.OnAfterTurn != null)
            Status.OnAfterTurn(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatsBoosts();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectivness { get; set; }
}