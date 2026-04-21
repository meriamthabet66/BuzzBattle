using Data.Data;
using TMPro;
using UnityEngine;

namespace UI {
    public class PlayerSetupPanelUI : MonoBehaviour
    {
        

        [Header("Where to go next?")]
        [SerializeField] private GameObject nextPanel; // Drag MatchConfigPanel here

        

        

        // Hook to your orange "Next/Continue" button
        public void OnClickNext()
        {
            MenuController.Instance.OpenPanel(nextPanel);
        }
    }
}