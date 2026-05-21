using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterChoiceUI : MonoBehaviour
    {
        
        [SerializeField] private Image characterImage;
        [SerializeField] private Image selectionOutline;
        [SerializeField] private Sprite SelectedOutline;
        [SerializeField] private Sprite NotSelectedOutline;
        
        private CharacterLinkPopup parentPopup;
        public long CharacterId { get; private set; } // The ID from the database

        public void Setup(CharacterData data, CharacterLinkPopup popup)
        {
            this.CharacterId = data.id;
            this.parentPopup = popup;

            // TODO: Load the 'character' sprite from data.skin_url
            // For now, it will just show the placeholder you have in the prefab.

            Deselect();
        }

        public void OnClick()
        {
            // Tell the main popup that I have been selected!
            parentPopup.OnCharacterSelected(this);
        }

        public void Select()
        {
            if (selectionOutline != null) selectionOutline.sprite = SelectedOutline;
        }

        public void Deselect()
        {
            if (selectionOutline != null) selectionOutline.sprite = NotSelectedOutline;
        }
    }
}