using Data.Data;
using UnityEngine;

namespace UI {
    public class HomePanelUI : MonoBehaviour
    {
        [Header("Where to go next?")]
        [SerializeField] private GameObject nextPanel; 

        public void OnClickPlay()
        {
            MatchSetupData.ResetData(); 
            
            // --- THE FIX: Transition from Lobby to Setup ---
            GameManager.Instance.ChangeState(GameState.Setup);
            
            MenuController.Instance.OpenPanel(nextPanel);
        }
    }
}