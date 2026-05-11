using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MustacheCustomUI : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    [SerializeField]
    private List<PositionedSprite> _spriteOptions;

    [SerializeField]
    private int SpriteIndex = 2;

    private bool _isActive = false;

    public void ToggleAccessory()
    {
        _isActive = !_isActive;

        if (_isActive)
        {
            ApplySprite();
        }
        else
        {
            RemoveSprite();
        }
    }

    private void ApplySprite()
    {
        var positionedSprite = _spriteOptions[SpriteIndex];

        _image.sprite = positionedSprite.Sprite;
        _image.rectTransform.anchoredPosition = positionedSprite.PositionModifier;
        _image.enabled = true;
    }

    private void RemoveSprite()
    {
        _image.sprite = null;
        _image.enabled = false; // hides it completely
    }

    public void EnableRandom()
    {
        SpriteIndex = Random.Range(0, _spriteOptions.Count);
        ApplySprite();
        _image.enabled = true;
    }

    public void Disable()
    {
        _image.sprite = null;
        _image.enabled = false;
    }
}