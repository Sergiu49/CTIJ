using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Color highlightColor;
    
    int curentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }
        
        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++curentSelection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --curentSelection;

        curentSelection = Mathf.Clamp(curentSelection, 0, 4);
        UpdateMoveSelection(curentSelection);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            onSelected?.Invoke(curentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < 5; i++)
        {
            if(i == selection)
                moveTexts[i].color = highlightColor;
            else
                moveTexts[i].color = Color.black;
        }
    }
}
