using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fov : MonoBehaviour, IPlayerTriggerable
{
    
    public void OnPlayerTriggered(PlayerController player)
    {
        GameControler.Instance.OnEnterTrainerView(GetComponentInParent<TrainerController>());
    }
    
}
