using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Data.DTO;
using Managers;
using System.Threading.Tasks;

namespace UI.Shop
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Carousel Controls")]
        [SerializeField] private Button leftBtn;
        [SerializeField] private Button rightBtn;
        [SerializeField] private GameObject characterCreationSection; // The Fox Display Group
        [SerializeField] private Button addCharacterBtn;     

        [Header("Accessories Scroll")]
        [SerializeField] private Transform accessoryContent;
        [SerializeField] private GameObject accessoryPrefab;

        [Header("Head Items")]
        [SerializeField] private GameObject topHatObj;

        [Header("Face Items")]
        [SerializeField] private GameObject moustacheObj;
        [SerializeField] private GameObject monocleObj;

        [Header("Neck Items")]
        [SerializeField] private GameObject tieObj;
        [SerializeField] private GameObject bowTieObj;

        [Header("Shop Buttons")]
        [SerializeField] private Button buttonSave;
        [SerializeField] private Button buttonDelete;
        [SerializeField] private Button buttonRand;
        
        [Header("Offline Protection")]
        [SerializeField] private GameObject shopUIContainer; 
        [SerializeField] private GameObject offlineMessagePanel; 

        
        private NetworkReachability lastReachability;
        private List<SavedCharacterDTO> playerCharacters = new List<SavedCharacterDTO>();
        private int currentIndex = 0; 
        
        private List<long> currentEquippedItemIds = new List<long>();
        private List<ItemDTO> catalogItems = new List<ItemDTO>();
        private List<OwnershipRegistryDTO> ownedItems = new List<OwnershipRegistryDTO>();

        private void Start()
        {
            lastReachability = Application.internetReachability;
            
            if (leftBtn != null) leftBtn.onClick.AddListener(GoLeft);
            if (rightBtn != null) rightBtn.onClick.AddListener(GoRight);
            if (addCharacterBtn != null) addCharacterBtn.onClick.AddListener(CreateNewCharacter);
            if (buttonSave != null) buttonSave.onClick.AddListener(OnSaveClicked);
            
            
            // --- NEW: Hook up the buttons ---
            if (buttonRand != null) buttonRand.onClick.AddListener(OnRandomizeClicked);
            if (buttonDelete != null) buttonDelete.onClick.AddListener(OnDeleteClicked);
        }
        
        private void Update()
        {
            // Only run this logic if the Shop Panel is currently visible
            if (!gameObject.activeInHierarchy) return;

            NetworkReachability currentReachability = Application.internetReachability;

            // Did the internet JUST turn off?
            if (lastReachability != NetworkReachability.NotReachable && 
                currentReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("<color=red>INTERNET LOST! Kicking out of Shop.</color>");
                
                // Hide the shop UI and show the warning panel instantly
                if (shopUIContainer != null) shopUIContainer.SetActive(false);
                if (offlineMessagePanel != null) offlineMessagePanel.SetActive(true);
            }
            // Did the internet JUST come back?
            else if (lastReachability == NetworkReachability.NotReachable && 
                     currentReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("<color=green>INTERNET RESTORED! Reloading Shop.</color>");
                
                if (shopUIContainer != null) shopUIContainer.SetActive(true);
                if (offlineMessagePanel != null) offlineMessagePanel.SetActive(false);
                
                // Fetch the freshest data just in case they missed something while offline
                _ = RefreshShop(currentIndex);
            }

            // Update our tracker
            lastReachability = currentReachability;
        }

        private async void OnEnable()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (shopUIContainer != null) shopUIContainer.SetActive(false);
                if (offlineMessagePanel != null) offlineMessagePanel.SetActive(true);
                return;
            }

            if (shopUIContainer != null) shopUIContainer.SetActive(true);
            if (offlineMessagePanel != null) offlineMessagePanel.SetActive(false);
            
            await RefreshShop(0);
        }

        // We add a parameter with a default value of 0
        public async Task RefreshShop(int targetIndex = 0)
        {
            if (PlayerManager.Instance.HostAccount == null) return;
            string hostId = PlayerManager.Instance.HostAccount.id;

            Debug.Log("Shop: Fetching Data...");

            // 1. Fetch Characters
            var charResponse = await SupabaseManager.Instance.Client
                .From<SavedCharacterDTO>().Where(x => x.profile_id == hostId).Get();
            
            playerCharacters = charResponse.Models ?? new List<SavedCharacterDTO>();

            // 2. Fetch Catalog & Owned Items
            var itemResponse = await SupabaseManager.Instance.Client.From<ItemDTO>().Get();
            if (itemResponse.Models != null) catalogItems = itemResponse.Models;

            var ownedResponse = await SupabaseManager.Instance.Client.From<OwnershipRegistryDTO>()
                .Where(x => x.profile_id == hostId)
                .Filter("character_id", Postgrest.Constants.Operator.Is, (object)null) 
                .Get();
            
            if (ownedResponse.Models != null) ownedItems = ownedResponse.Models;

            // --- THE FIX: Use the target index, but make sure it doesn't go out of bounds! ---
            if (targetIndex >= 0 && targetIndex <= playerCharacters.Count)
            {
                currentIndex = targetIndex;
            }
            else
            {
                currentIndex = 0;
            }

            PopulateAccessoryGrid();
            UpdateCarouselDisplay();
        }


        // --- CAROUSEL NAVIGATION ---

        private void GoLeft()
        {
            // Even if we are at the [+] button (currentIndex == playerCharacters.Count), we should be able to go left!
            if (currentIndex > 0)
            {
                currentIndex--;
                Debug.Log($"Moved Left. Current Index: {currentIndex}");
                UpdateCarouselDisplay();
            }
        }

        private void GoRight()
        {
            if (currentIndex < playerCharacters.Count)
            {
                currentIndex++;
                UpdateCarouselDisplay();
            }
        }

        private async void UpdateCarouselDisplay()
        {
            characterCreationSection.SetActive(false);
            if (addCharacterBtn != null) addCharacterBtn.gameObject.SetActive(false);
            
            if (leftBtn != null) leftBtn.interactable = (currentIndex > 0);
            if (rightBtn != null) rightBtn.interactable = (currentIndex <= playerCharacters.Count);

            if (currentIndex == playerCharacters.Count)
            {
                if (addCharacterBtn != null) addCharacterBtn.gameObject.SetActive(true);
                currentEquippedItemIds.Clear();
                
                RefreshAccessorySelectionsOnly(); 
                TurnOffAllClothes();
                return;
            }

            characterCreationSection.SetActive(true);
            var activeFox = playerCharacters[currentIndex];

            var outfitResponse = await SupabaseManager.Instance.Client.From<OwnershipRegistryDTO>()
                .Where(x => x.character_id == activeFox.id).Get();
            
            currentEquippedItemIds.Clear();
            if (outfitResponse.Models != null)
            {
                foreach (var item in outfitResponse.Models) currentEquippedItemIds.Add(item.item_id);
            }

            // --- THE FIX: We just refresh the borders, no destroying! ---
            RefreshAccessorySelectionsOnly(); 
            UpdateFoxVisuals();
        }
        // --- ACCESSORY LOGIC ---

        // We only call this ONCE in RefreshShop()
        private void PopulateAccessoryGrid()
        {
            // Delete old items
            foreach (Transform child in accessoryContent) Destroy(child.gameObject);
            
            if (catalogItems == null || accessoryPrefab == null) return;

            // Spawn the items once
            foreach (var item in catalogItems)
            {
                GameObject newObj = Instantiate(accessoryPrefab, accessoryContent);
                var itemUI = newObj.GetComponent<AccessoryItemUI>();
                
                // Set their initial state (unowned, unequipped)
                if (itemUI != null) itemUI.Setup(item, this, false, false);
            }
        }


        // --- NEW METHOD: Call this when clicking Left/Right instead of PopulateAccessoryGrid ---
        private void RefreshAccessorySelectionsOnly()
        {
            // --- SAFETY CHECK: If there is nothing in the container, do not crash! ---
            if (accessoryContent == null || accessoryContent.childCount == 0) return;

            foreach (Transform child in accessoryContent)
            {
                var itemUI = child.GetComponent<AccessoryItemUI>();
                if (itemUI != null && itemUI.ItemData != null)
                {
                    bool isOwned = ownedItems.Exists(link => link.item_id == itemUI.ItemData.id);
                    bool isEquipped = currentEquippedItemIds.Contains(itemUI.ItemData.id);
                    
                    // We call Setup again, which will quickly swap the borders and lock icons
                    // without needing to redownload the image!
                    itemUI.Setup(itemUI.ItemData, this, isOwned, isEquipped);
                }
            }
        }

        public void ToggleEquip(AccessoryItemUI itemUI)
        {
            if (currentIndex == playerCharacters.Count) return; 

            long itemId = itemUI.ItemData.id;

            // If we already have it equipped, just take it off.
            if (currentEquippedItemIds.Contains(itemId)) 
            {
                currentEquippedItemIds.Remove(itemId);
                Debug.Log($"Unequipped: {itemUI.ItemData.item_name}");
            }
            else 
            {
                // --- THE EXCLUSIVITY FIX ---
                // Before equipping, remove any items that use the same slot!
                string newType = itemUI.ItemData.item_type.ToLower();
                
                // If it's a neck item, remove all other neck items
                if (newType == "neck") 
                {
                    currentEquippedItemIds.RemoveAll(id => {
                        var existingItem = catalogItems.Find(c => c.id == id);
                        return existingItem != null && existingItem.item_type.ToLower() == "neck";
                    });
                }
                // (You can add the same logic for "face" or "hat" if you want them exclusive too!)

                currentEquippedItemIds.Add(itemId);
                Debug.Log($"Equipped: {itemUI.ItemData.item_name}");
            }

            // Immediately update visuals
            RefreshAccessorySelectionsOnly();
            UpdateFoxVisuals();
        }

        private void TurnOffAllClothes()
        {
            if (topHatObj != null) topHatObj.SetActive(false);
            if (moustacheObj != null) moustacheObj.SetActive(false);
            if (monocleObj != null) monocleObj.SetActive(false);
            if (tieObj != null) tieObj.SetActive(false);
            if (bowTieObj != null) bowTieObj.SetActive(false);
        }

        private void UpdateFoxVisuals()
        {
            TurnOffAllClothes();

            foreach (long id in currentEquippedItemIds)
            {
                ItemDTO item = catalogItems.Find(x => x.id == id);
                if (item == null) continue;

                // Make sure your database 'item_name' matches these strings exactly!
                switch (item.item_name)
                {
                    case "قبعة": if (topHatObj != null) topHatObj.SetActive(true); break;
                    case "شارب": if (moustacheObj != null) moustacheObj.SetActive(true); break;
                    case "مونوكل": if (monocleObj != null) monocleObj.SetActive(true); break;
                    case "ربطة عنق": if (tieObj != null) tieObj.SetActive(true); break;
                    case "ربطة عنق الفراشة": if (bowTieObj != null) bowTieObj.SetActive(true); break;
                }
            }
        }

        // --- DATABASE ACTIONS ---

     public async void TryPurchaseItem(AccessoryItemUI itemUI)
        {
            long itemId = itemUI.ItemData.id;
            int price = itemUI.ItemData.price;
            
            var savedAccount = LocalAccountManager.Instance.SavedAccount;
            if (savedAccount.Stars < price) return;

            // 1. Deduct Stars
            int newStars = savedAccount.Stars - price;
            string hostId = PlayerManager.Instance.HostAccount.id;

            // 2. Update the Cloud (Using the RPC so it updates last_updated!)
            var parameters = new Dictionary<string, object>
            {
                { "p_id", hostId },
                { "p_stars", newStars },
                { "p_score", savedAccount.Score },
                { "p_matches_played", savedAccount.MatchPlayedCount },
                { "p_match_wins", savedAccount.MatchWinCount },
                { "p_tournament_wins", savedAccount.TournamentWinCount },
                { "p_correct_steals", savedAccount.CorrectSteals },
                { "p_total_steals", savedAccount.Steals },
                { "p_last_updated", System.DateTime.UtcNow }
            };
            await SupabaseManager.Instance.Client.Rpc("update_player_stats", parameters);

            // 3. Add to Database Closet
            var purchaseRecord = new OwnershipRegistryDTO {
                profile_id = hostId, item_id = itemId
                // removed is_locked!
            };
            var insertResponse = await SupabaseManager.Instance.Client.From<OwnershipRegistryDTO>().Insert(purchaseRecord);

            if (insertResponse.Models != null && insertResponse.Models.Count > 0)
            {
                ownedItems.Add(insertResponse.Models[0]);
            }

            // --- THE FIX: Update the Local Wallet AND save it to disk! ---
            
            // 4a. Update the local variable
            LocalAccountManager.Instance.SavedAccount.Stars = newStars;
            LocalAccountManager.Instance.SavedAccount.last_updated = System.DateTime.UtcNow;
            
            // 4b. Tell LocalAccountManager to write the new 50 stars to the JSON file
            // (We will add this public method to LocalAccountManager next)
            LocalAccountManager.Instance.ForceSaveToDisk(); 

            // 5. Update the UI
            PlayerManager.Instance.UpdateHostStars(newStars);
            
            // Auto-equip
            if (currentIndex < playerCharacters.Count && !currentEquippedItemIds.Contains(itemId)) 
            {
                ToggleEquip(itemUI);
            }
            else
            {
                RefreshAccessorySelectionsOnly();
            }
        }
        public async void CreateNewCharacter()
        {
            if (addCharacterBtn != null) addCharacterBtn.interactable = false;
            string hostId = PlayerManager.Instance.HostAccount.id;
            
            var newChar = new SavedCharacterDTO {
                profile_id = hostId,
                nickname = "ثعلب جديد", // "New Fox"
                skin_url = "" 
            };

            await SupabaseManager.Instance.Client.From<SavedCharacterDTO>().Insert(newChar);
            
            if (addCharacterBtn != null) addCharacterBtn.interactable = true;

            // --- THE FIX: Tell RefreshShop to jump to the newly created Fox! ---
            // If we previously had 3 foxes, playerCharacters.Count was 3. 
            // The new fox will be placed at Index 3 (the 4th slot).
            int newFoxIndex = playerCharacters.Count;
            
            await RefreshShop(newFoxIndex);
        }

        private async void OnSaveClicked()
        {
            if (currentIndex == playerCharacters.Count) return;

            if (buttonSave != null) buttonSave.interactable = false;
            var activeFox = playerCharacters[currentIndex];
            string hostId = PlayerManager.Instance.HostAccount.id;

            await SupabaseManager.Instance.Client.From<OwnershipRegistryDTO>()
                .Where(x => x.character_id == activeFox.id).Delete();

            foreach (long itemId in currentEquippedItemIds)
            {
                var outfitRecord = new OwnershipRegistryDTO {
                    profile_id = hostId,
                    character_id = activeFox.id, 
                    item_id = itemId
                };
                await SupabaseManager.Instance.Client.From<OwnershipRegistryDTO>().Insert(outfitRecord);
            }

            Debug.Log("<color=green>Outfit Saved to Cloud!</color>");
            if (buttonSave != null) buttonSave.interactable = true;
        }
        
        
        
        // --- ADD THIS METHOD to ShopManager.cs ---
        private void OnRandomizeClicked()
        {
            if (currentIndex == playerCharacters.Count) return;
            if (ownedItems == null || ownedItems.Count == 0) return;

            currentEquippedItemIds.Clear();

            // Group owned items by their TYPE (not name)
            Dictionary<string, List<long>> itemsByType = new Dictionary<string, List<long>>();
            foreach (var owned in ownedItems)
            {
                ItemDTO catalogData = catalogItems.Find(x => x.id == owned.item_id);
                if (catalogData != null)
                {
                    string type = catalogData.item_type.ToLower(); // 'neck', 'hat', 'face'
                    if (!itemsByType.ContainsKey(type)) itemsByType[type] = new List<long>();
                    
                    itemsByType[type].Add(catalogData.id);
                }
            }

            // Pick exactly ONE item per body part (or zero)
            foreach (var typeList in itemsByType.Values)
            {
                // 30% chance to leave this body part empty for variety!
                if (UnityEngine.Random.value < 0.3f) continue; 

                int randomPick = UnityEngine.Random.Range(0, typeList.Count);
                currentEquippedItemIds.Add(typeList[randomPick]);
            }

            RefreshAccessorySelectionsOnly();
            UpdateFoxVisuals();
        }
        
        // --- ADD THIS METHOD to ShopManager.cs ---
        private async void OnDeleteClicked()
        {
            // Do not delete if we are on the [+] New Character screen
            if (currentIndex == playerCharacters.Count) return;

            // Safety check: Prevent deleting the very last character
            if (playerCharacters.Count <= 1)
            {
                Debug.LogWarning("You cannot delete your only character!");
                // Optional: Show an error message on the screen
                return;
            }

            buttonDelete.interactable = false;
            var activeFox = playerCharacters[currentIndex];
            Debug.Log($"Deleting character: {activeFox.nickname}...");

            try
            {
                // 1. Delete from Supabase
                await SupabaseManager.Instance.Client.From<SavedCharacterDTO>()
                    .Where(x => x.id == activeFox.id).Delete();

                // 2. Remove from local list
                playerCharacters.RemoveAt(currentIndex);

                // 3. Adjust the index so we don't go out of bounds
                if (currentIndex >= playerCharacters.Count)
                {
                    currentIndex = playerCharacters.Count - 1;
                }

                Debug.Log("<color=red>Character Deleted Successfully!</color>");
                
                // 4. Update the screen to show the new Fox at this index
                UpdateCarouselDisplay();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to delete character: " + e.Message);
            }
            finally
            {
                buttonDelete.interactable = true;
            }
        }
    }
}