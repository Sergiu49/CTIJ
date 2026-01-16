using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    
    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        
        nameText.text = pokemon.@base.Name;
        levelText.text = "Lvl " + pokemon.level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHP);
    }
}
