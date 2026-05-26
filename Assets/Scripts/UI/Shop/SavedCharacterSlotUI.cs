// using UnityEngine;
// using UnityEngine.UI;
// using Data;
//
// namespace UI.Shop
// {
//     public class ShopCharacterSlotUI : MonoBehaviour
//     {
//         [SerializeField] private Image characterImage;        // Drag the child 'character' image here
//         [SerializeField] private GameObject selectionOutline; // Drag 'SelectionOutline' here
//         
//         public CharacterData CharacterData { get; private set; }
//         private ShopManager shopManager;
//
//         public void Setup(CharacterData data, ShopManager manager, bool isSelected)
//         {
//             this.CharacterData = data;
//             this.shopManager = manager;
//             
//             if (selectionOutline != null) selectionOutline.SetActive(isSelected);
//         }
//
//         public void OnClick()
//         {
//             shopManager.SelectCharacter(this);
//         }
//     }
// }