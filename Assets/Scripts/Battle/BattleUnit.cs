using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }
    
    Image image;
    Vector3 originalPosition;
    Color originalColor;

    public void Awake()
    {
        image =  GetComponent<Image>();
        originalPosition = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup()
    {
        Pokemon = new Pokemon(_base, level);
        if (isPlayerUnit)
            GetComponent<Image>().sprite = Pokemon.Base.BackSprite;
        else
            GetComponent<Image>().sprite = Pokemon.Base.FrontSprite;
        
        image.color = originalColor;
        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        if(isPlayerUnit)
            image.transform.localPosition = new Vector3(-500, originalPosition.y);
        else 
            image.transform.localPosition = new Vector3(500, originalPosition.y);
        
        image.transform.DOLocalMoveX(originalPosition.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence =  DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x - 50f, 0.25f));
        
        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x, 0.25f));
    }

    public void PlayHurtAnimation()
    {
        var sequence =  DOTween.Sequence();
        sequence.Append(image.DOColor(Color.grey, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayDeadAnimation()
    {
        var sequence =  DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f)); //folosim join ca sa intre in acelasi timp cu cealalta animatie, append doar adauga la coada
    }
}
