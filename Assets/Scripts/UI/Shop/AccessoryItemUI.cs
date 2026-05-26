using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using TMPro;
using RTLTMPro;
using Data.DTO;

namespace UI.Shop
{
    public class AccessoryItemUI : MonoBehaviour
    {
        [Header("Hierarchy Links")]
        [SerializeField] private GameObject selection;         // Drag 'selection' here
        [SerializeField] private RTLTextMeshPro accesoryName;  // Drag 'accesoryName' here
        [SerializeField] private Image accesoryImage;          // Drag 'accesoryImage' here
        [SerializeField] private GameObject lockedOverlay;     // Drag 'lockedOverlay' here
        [SerializeField] private TMP_Text priceText;           // Drag 'priceText' (inside Price) here

        public ItemDTO ItemData { get; private set; }
        private ShopManager shopManager;
        private bool isOwned = false;

        public async void Setup(ItemDTO data, ShopManager manager, bool owned, bool equipped)
        {
            this.ItemData = data;
            this.shopManager = manager;
            this.isOwned = owned;

            if (accesoryName != null) accesoryName.text = data.item_name;
            if (priceText != null) priceText.text = data.price.ToString();

            if (lockedOverlay != null) lockedOverlay.SetActive(!owned);
            if (selection != null) selection.SetActive(equipped);

            // --- THE SMOOTH FIX ---
            // Only show the image AFTER it finishes downloading
            if (!string.IsNullOrEmpty(data.item_image_url) && accesoryImage != null)
            {
                Sprite downloadedSprite = await DownloadSprite(data.item_image_url);
                if (downloadedSprite != null) 
                {
                    accesoryImage.sprite = downloadedSprite;
                    accesoryImage.color = Color.white; // Turn Alpha back to 1!
                }
            }
        }

        public void OnClick()
        {
            // We removed the 'async' here to prevent weird double-clicks.
            // The ShopManager handles the async logic!
            if (!isOwned) 
            {
                shopManager.TryPurchaseItem(this);
            }
            else 
            {
                shopManager.ToggleEquip(this);
            }
        }

        private async Task<Sprite> DownloadSprite(string url)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                return null;
            }
        }
    }
}