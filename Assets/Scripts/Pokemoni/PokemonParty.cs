using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
    }

    private void Start()
    {
        foreach (Pokemon pokemon in pokemons)
        {
            pokemon.init();
        }
    }

    public Pokemon GetHealtyPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }
}
