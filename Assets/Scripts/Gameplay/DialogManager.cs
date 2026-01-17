using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;
    
    public event Action OnShowDialog;
    public event Action OnCloseDialog;
    public static DialogManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    Dialog dialog;
    Action onDialogFinished;
        
    int currentLine=0;
    bool isTyping;
    
    public bool isShowing { get; private set; }
    public IEnumerator ShowDialog(Dialog dialog, Action onfinished=null)
    {

        yield return new WaitForEndOfFrame();
        OnShowDialog?.Invoke();

        isShowing = true;
        this.dialog= dialog;
        onDialogFinished = onfinished;
       
        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    public void HandleUpdate()
    {

        if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
        {

            ++currentLine;
            if (currentLine < dialog.Lines.Count)
            {
                
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
                
            }
            else
            {
                
                currentLine = 0;
                isShowing = false;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke();
                
            }

        }
        
    }
    public IEnumerator TypeDialog(string line)
    {
        
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
            
        }
        isTyping = false;
    }
    
}
