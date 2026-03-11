using UnityEngine;

public class PlayerSetupState : MonoBehaviour
{
    private PlayerSetupUI ui;

    public PlayerSetupState(PlayerSetupUI ui) {
        this.ui = ui;
    }

    public void Enter() {
        ui.Show();
    }

    public void Exit() {
        ui.Hide();
    }

    public void Update() { }
}
