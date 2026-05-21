using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RTLTMPro;
using Data.DTO;

namespace UI
{
    public class GlobalLeaderboardRowUI : MonoBehaviour
    {
        [Header("Hierarchy: Points")]
        [SerializeField] private RTLTextMeshPro pointsText;      // Drag 'Points' here

        [Header("Hierarchy: info")]
        [SerializeField] private RTLTextMeshPro nameText;        // Drag 'info/name' here
        [SerializeField] private RTLTextMeshPro matchText;       // Drag 'info/match' here
        [SerializeField] private GameObject characterOutline;    // Drag 'info/character/outline' here

        [Header("Hierarchy: order")]
        [SerializeField] private Image medalIcon;                // Drag 'order/icon' here
        [SerializeField] private TMP_Text rankNumberText;        // Drag your new rank Text here

        [Header("Medal Sprites")]
        [SerializeField] private Sprite goldMedal;
        [SerializeField] private Sprite silverMedal;
        [SerializeField] private Sprite bronzeMedal;

        public void Setup(ProfileDTO data, int rank)
        {
            // 1. Set the Name
            if (nameText != null) nameText.text = data.username;

            // 2. Set the Stats (Raw Arabic text - RTLTMPro handles the connection)
            if (matchText != null) 
                matchText.text = data.matches_played + " مباراة";

            if (pointsText != null) 
                pointsText.text = data.score + " نقطة";

            // 3. THE WINNER OUTLINE: Only active for Rank 1
            if (characterOutline != null)
            {
                characterOutline.SetActive(rank == 1);
            }

            // 4. Handle the 'order' section (Medals vs Numbers)
            if (rank <= 3)
            {
                if (rankNumberText != null) rankNumberText.gameObject.SetActive(false);
                if (medalIcon != null)
                {
                    medalIcon.gameObject.SetActive(true);
                    if (rank == 1) medalIcon.sprite = goldMedal;
                    else if (rank == 2) medalIcon.sprite = silverMedal;
                    else if (rank == 3) medalIcon.sprite = bronzeMedal;
                }
            }
            else
            {
                if (medalIcon != null) medalIcon.gameObject.SetActive(false);
                if (rankNumberText != null)
                {
                    rankNumberText.gameObject.SetActive(true);
                    rankNumberText.text = rank.ToString();
                }
            }
        }
    }
}