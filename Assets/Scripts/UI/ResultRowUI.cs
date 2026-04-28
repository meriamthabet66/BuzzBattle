using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

namespace UI
{
    public class ResultRowUI : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro playerNameText;
        [SerializeField] private RTLTextMeshPro roundScoreText;
        [SerializeField] private RTLTextMeshPro totalScoreText;
        
        [SerializeField] private GameObject trophyIcon; // For 1st place
        [SerializeField] private GameObject highlightOutline; // The orange outline for 1st place

        public void Setup(PlayerData data, bool isFirstPlace)
        {
            gameObject.SetActive(true); // Make sure the row is visible

            playerNameText.text = data.DisplayName; // Already fixed previously!
            roundScoreText.text = data.RoundScore.ToString();
            totalScoreText.text = data.TotalScore.ToString();

            // Only show the trophy and orange outline if they are in 1st place!
            if (trophyIcon != null) trophyIcon.SetActive(isFirstPlace);
            if (highlightOutline != null) highlightOutline.SetActive(isFirstPlace);
        }
    }
}