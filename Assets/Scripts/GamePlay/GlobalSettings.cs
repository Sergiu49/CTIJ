using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color hightlightedColor;
    
    public Color HightlightedColor => hightlightedColor;
    
    public static GlobalSettings i {get; private set;}

    private void Awake()
    {
        i = this;
    }
}
