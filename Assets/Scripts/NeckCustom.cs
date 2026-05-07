using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NeckCustomUI : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private List<PositionedSprite> _spriteOptions;

    [SerializeField] private int _tieIndex = 4;
    [SerializeField] private int _bowtieIndex = 5;

    private enum NeckState
    {
        None,
        Tie,
        Bowtie
    }

    private NeckState _state = NeckState.None;

    private void Awake()
    {
        SetNone();
    }

    public void ToggleTie()
    {
        if (_state == NeckState.Tie)
            SetNone();
        else
            SetTie();
    }

    public void ToggleBowtie()
    {
        if (_state == NeckState.Bowtie)
            SetNone();
        else
            SetBowtie();
    }

    private void ClearImage()
    {
        // FULL RESET
        _image.enabled = false;
        _image.sprite = null;
        _image.color = Color.clear;

        // Force redraw reset
        _image.SetAllDirty();
    }

    private void SetNone()
    {
        _state = NeckState.None;
        ClearImage();
    }

    private void SetTie()
    {
        _state = NeckState.Tie;
        Apply(_tieIndex);
    }

    private void SetBowtie()
    {
        _state = NeckState.Bowtie;
        Apply(_bowtieIndex);
    }

    private void Apply(int index)
    {
        if (index < 0 || index >= _spriteOptions.Count)
            return;

        // HARD RESET FIRST
        ClearImage();

        PositionedSprite positionedSprite = _spriteOptions[index];

        _image.sprite = positionedSprite.Sprite;
        _image.rectTransform.anchoredPosition = positionedSprite.PositionModifier;

        _image.color = Color.white;
        _image.enabled = true;

        // Force UI refresh
        _image.SetAllDirty();
    }

    public void RandomizeNeck()
    {
        // Prevent previous frame overlap
        ClearImage();

        int choice = Random.Range(0, 3);

        switch (choice)
        {
            case 0:
                SetNone();
                break;

            case 1:
                SetTie();
                break;

            case 2:
                SetBowtie();
                break;
        }
    }
}