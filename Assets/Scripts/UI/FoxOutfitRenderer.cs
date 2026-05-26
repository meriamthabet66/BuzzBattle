using UnityEngine;
using Data;
using Data.DTO;
using Managers;

namespace UI
{
    public class FoxOutfitRenderer : MonoBehaviour
    {
        [Header("Head Items")]
        [SerializeField] private GameObject topHatObj;

        [Header("Face Items")]
        [SerializeField] private GameObject moustacheObj;
        [SerializeField] private GameObject monocleObj;

        [Header("Neck Items")]
        [SerializeField] private GameObject tieObj;
        [SerializeField] private GameObject bowTieObj;

        public async void RenderOutfit(CharacterData foxData)
        {
            // 1. Turn off all clothes layers to start fresh (Naked Fox)
            if (topHatObj != null) topHatObj.SetActive(false);
            if (moustacheObj != null) moustacheObj.SetActive(false);
            if (monocleObj != null) monocleObj.SetActive(false);
            if (tieObj != null) tieObj.SetActive(false);
            if (bowTieObj != null) bowTieObj.SetActive(false);

            // --- THE FIX: If there is no character data, STOP HERE! ---
            // The Base Fox image (parent) stays visible, but it has no clothes.
            if (foxData == null || foxData.id == -1) return; 

            // 2. Fetch the outfit this specific Fox is wearing from the Database
            var outfitResponse = await SupabaseManager.Instance.Client.From<OwnershipRegistryDTO>()
                .Where(x => x.character_id == foxData.id).Get();

            if (outfitResponse.Models == null || outfitResponse.Models.Count == 0) return;

            // 3. Fetch the Catalog
            var catalogResponse = await SupabaseManager.Instance.Client.From<ItemDTO>().Get();
            if (catalogResponse.Models == null) return;

            // 4. Draw the clothes!
            foreach (var outfitItem in outfitResponse.Models)
            {
                ItemDTO itemDetails = catalogResponse.Models.Find(x => x.id == outfitItem.item_id);
                if (itemDetails == null) continue;

                switch (itemDetails.item_name)
                {
                    case "قبعة": if (topHatObj != null) topHatObj.SetActive(true); break;
                    case "شارب": if (moustacheObj != null) moustacheObj.SetActive(true); break;
                    case "مونوكل": if (monocleObj != null) monocleObj.SetActive(true); break;
                    case "ربطة عنق": if (tieObj != null) tieObj.SetActive(true); break;
                    case "ربطة عنق الفراشة": if (bowTieObj != null) bowTieObj.SetActive(true); break;
                }
            }
        }
    }
}