using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string _name;
    
    [TextArea]
    [SerializeField] string description;
    
    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effect;
    [SerializeField] List<SecondaryEffects> secondarieses;
    [SerializeField] MoveTarget target;

    // Properties to expose the variables
    public string Name {
        get { return _name; }
    }

    public string Description {
        get { return description; }
    }

    public PokemonType Type {
        get { return type; }
    }

    public int Power {
        get { return power; }
    }

    public int Accuracy {
        get { return accuracy; }
    }
    
    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }

    public int PP {
        get { return pp; }
    }
    
    public int Priority {
        get { return priority; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effect
    {
        get { return effect; }
    }

    public List<SecondaryEffects> Secondaries
    {
        get { return secondarieses; }
    }
    
    public MoveTarget Target
    {
        get { return target; }
    }
    
}
[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionsID status;
    [SerializeField] ConditionsID volatileStatus;
    
    public List<StatBoost> Boosts {
        get { return boosts; }
    }

    public ConditionsID Status
    {
        get { return status; }
    }
    
    public ConditionsID VolatileStatus
    {
        get { return volatileStatus; }
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance
    {
        get => chance;
    }
    
    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}
public enum MoveCategory
{
    Physical, Special, Status
}

public enum MoveTarget
{
    Foe, Self
}