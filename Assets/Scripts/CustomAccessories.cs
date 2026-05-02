using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomAccessoriesUI : MonoBehaviour
{
    [SerializeField]
    private Image _image;

    [SerializeField]
    private List<PositionedSprite> _spriteOptions;

    [field: SerializeField]
    public int SpriteIndex { get; private set; }

    [ContextMenu("Next Sprite")]
    public void NextSprite() 
    {
        SpriteIndex = Mathf.Min(SpriteIndex + 1, _spriteOptions.Count - 1);
        UpdateSprite();
        //return _spriteOptions[SpriteIndex];
    }

    [ContextMenu("Previous Sprite")]
    public void PreviousSprite()
    {
        SpriteIndex = Mathf.Max(SpriteIndex - 1, 0);
        UpdateSprite();
        //return _spriteOptions[SpriteIndex];
    }

    [ContextMenu("Randomize")]
    public void Randomize()
    {
        SpriteIndex = Random.Range(0, _spriteOptions.Count);
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        SpriteIndex = Mathf.Clamp(SpriteIndex, 0, _spriteOptions.Count - 1);
        var positionedSprite = _spriteOptions[SpriteIndex];

        _image.sprite = positionedSprite.Sprite;

        RectTransform rectTransform = _image.rectTransform;
        rectTransform.anchoredPosition = positionedSprite.PositionModifier;
    }
}


/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomAccessories : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private List<PositionedSprite> _spriteOptions;

    [field: SerializeField]
    public int SpriteIndex { get; private set; }

    [ContextMenu("Next Sprite")]
    public PositionedSprite NextSprite() 
    {
        SpriteIndex = Mathf.Min (SpriteIndex + 1, _spriteOptions.Count - 1);
        UpdateSprite();
        return _spriteOptions[SpriteIndex];
    }

    [ContextMenu("Previous Sprite")]
    public PositionedSprite PreviousSprite()
    {
        SpriteIndex = Mathf.Max(SpriteIndex - 1, 0);
        UpdateSprite();
        return _spriteOptions[SpriteIndex];
    }

    [ContextMenu("Randomize")]
    public void Randomize()
    {
        SpriteIndex = Random.Range(0, _spriteOptions.Count -1);
        UpdateSprite();
    }
    
    private void UpdateSprite()
    {
        SpriteIndex = Mathf.Clamp(SpriteIndex, 0, _spriteOptions.Count - 1);
        var positionedSprite = _spriteOptions[SpriteIndex];
        _spriteRenderer.sprite = positionedSprite.Sprite;
        transform.localPosition = positionedSprite.PositionModifier;
    }
}
*/