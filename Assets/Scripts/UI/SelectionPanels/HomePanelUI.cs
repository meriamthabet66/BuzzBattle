using Data.Data;
using UnityEngine;

namespace UI {
    public class HomePanelUI : MonoBehaviour
    {
        [Header("Where to go next?")]
        [SerializeField] private GameObject nextPanel; // Drag GameModePanel here

        // Hook this to your big "Play" or "Start" button
        public void OnClickPlay()
        {
            // Resets old data so a new game starts fresh!
            MatchSetupData.ResetData(); 
            
            MenuController.Instance.OpenPanel(nextPanel);
        }
    }
}