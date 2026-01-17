using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    
    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    
    
    Pokemon _pokemon;
    private Dictionary<ConditionsID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        
        nameText.text = pokemon.@base.Name;
        levelText.text = "Lvl " + pokemon.level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHP);
        statusColors = new Dictionary<ConditionsID, Color>()
        {
            { ConditionsID.psn, psnColor },
            { ConditionsID.brn, brnColor },
            { ConditionsID.slp, slpColor },
            { ConditionsID.par, parColor },
            { ConditionsID.frz, frzColor },
        };
        
        SetStatusText();
        _pokemon.OnStatusChange += SetStatusText;
    }

    void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHP);
            _pokemon.HpChanged = false;
        }

    }
    
    
}
