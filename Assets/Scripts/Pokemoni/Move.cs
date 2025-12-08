using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    // Shorthand property syntax. 
    // This creates a private variable behind the scenes and exposes it.
    public MoveBase Base { get; set; }
    public int PP { get; set; }

    // Constructor to initialize the move based on the MoveBase
    public Move(MoveBase pBase)
    {
        Base = pBase;
        PP = pBase.PP; // Start with max PP
    }
}