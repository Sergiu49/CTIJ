using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;
     [SerializeField] BattleHud enemyHud;

   private void Start()
    {
        SetupBattle();
    }

    public void SetupBattle()
    {

        enemyUnit.Setup();
        playerUnit.Setup();
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);
    }

   
}
