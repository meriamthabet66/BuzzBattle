using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterChoiceUI : MonoBehaviour
    {
        [SerializeField] private Image selectionOutline;
        [SerializeField] private Sprite SelectedOutline;
        [SerializeField] private Sprite NotSelectedOutline;
        
        // --- NEW: Drag your layered Fox prefab here ---
        [SerializeField] private FoxOutfitRenderer outfitRenderer; 
        
        private CharacterLinkPopup parentPopup;
        public long CharacterId { get; private set; } 
        
        

        public void Setup(CharacterData data, CharacterLinkPopup popup)
        {
            this.CharacterId = data.id;
            this.parentPopup = popup;

            // Draw the clothes!
            if (outfitRenderer != null)
            {
                outfitRenderer.gameObject.SetActive(true);
                outfitRenderer.RenderOutfit(data);
            }
            Deselect();
        }

        public void OnClick()
        {
            parentPopup.OnCharacterSelected(this);
        }

        public void Select() { if (selectionOutline != null) selectionOutline.sprite = SelectedOutline; }
        public void Deselect() { if (selectionOutline != null) selectionOutline.sprite = NotSelectedOutline; }
    }
}