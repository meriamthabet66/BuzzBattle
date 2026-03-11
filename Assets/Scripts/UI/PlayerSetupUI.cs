using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerSetupUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown playerCountDropdown;
    public Transform inputContainer;
    public GameObject inputPrefab;
    public Button startButton;

    private List<TMP_InputField> inputs = new();

    private void Start()
    {
        playerCountDropdown.onValueChanged.AddListener(UpdateInputs);
        startButton.onClick.AddListener(OnStartClicked);

        UpdateInputs(0);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    void UpdateInputs(int index)
    {
        int count = index + 2; // 2–4 players

        foreach (Transform child in inputContainer)
            Destroy(child.gameObject);

        inputs.Clear();

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(inputPrefab, inputContainer);
            var input = go.GetComponent<TMP_InputField>();
            input.placeholder.GetComponent<TextMeshProUGUI>().text = $"Player {i + 1}";
            inputs.Add(input);
        }
    }

    void OnStartClicked()
    {
        if (inputs.Count < 2)
        {
            Debug.Log("Minimum 2 players required");
            return;
        }

        foreach (var input in inputs)
        {
            if (string.IsNullOrWhiteSpace(input.text))
            {
                Debug.Log("All players must have names");
                return;
            }
        }

        Debug.Log("Game starting with valid players");

        // Transition to next state here
    }
}