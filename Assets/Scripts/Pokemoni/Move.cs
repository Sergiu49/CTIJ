using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{

  
    public MoveBase Base { get; set; } // punem getere si setere pt urmatorile implementari
    public int PP { get; set; }

   
    public Move(MoveBase pBase) // in constructor ii dam valorile din scripable obeject a miscari si pt simplitate pp il aduagam ca o valoare separata sa verificam cate miscari mai are
    {
        Base = pBase;
        PP = pBase.PP; 

    }
}