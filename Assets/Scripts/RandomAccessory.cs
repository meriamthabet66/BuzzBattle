using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RandomAccessoryUI : MonoBehaviour
{
    [SerializeField] private HatCustomUI _hat;
    [SerializeField] private MustacheCustomUI _mustache;
    [SerializeField] private MonocleCustomUI _monocle;
    [SerializeField] private NeckCustomUI _neck;

    private void RandomizeHat()
    {
        if (Random.value > 0.5f) _hat.EnableRandom();
        else _hat.Disable();
    }

    private void RandomizeMustache()
    {
        if (Random.value > 0.5f) _mustache.EnableRandom();
        else _mustache.Disable();
    }

    private void RandomizeMonocle()
    {
        if (Random.value > 0.5f) _monocle.EnableRandom();
        else _monocle.Disable();
    }

    public void RandomizeAll()
    {
        RandomizeHat();
        RandomizeMustache();
        RandomizeMonocle();

        _neck.RandomizeNeck();
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