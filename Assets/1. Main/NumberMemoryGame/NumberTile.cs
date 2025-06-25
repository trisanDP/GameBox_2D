// NumberTile.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberTile : MonoBehaviour {
    [Header("Visuals")]
    public Image backgroundImage;
    public TMP_Text numberText;
    public Button button;

    public int Number { get; private set; }
    private System.Action<NumberTile> onSelected;
    private bool isHidden = false;
    private bool isClickable = true;

    /// <summary>
    /// Initialize the tile with a number and callback.
    /// </summary>
    public void Initialize(int number, System.Action<NumberTile> callback) {
        // Clean up any previous listeners
        if(button != null)
            button.onClick.RemoveAllListeners();

        Number = number;
        numberText.text = number.ToString();
        onSelected = callback;
        isHidden = false;
        isClickable = true;

        // Reset visuals
        backgroundImage.color = Color.white;
        numberText.gameObject.SetActive(true);

        // Assign button listener to invoke the same selection logic
        if(button != null)
            button.onClick.AddListener(OnButtonClick);
    }

    /// <summary>
    /// Hide the number, showing only the square.
    /// </summary>
    public void HideNumber() {
        isHidden = true;
        numberText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shared click handler for the UI Button.
    /// </summary>
    private void OnButtonClick() {
        if(!isClickable) return;
        onSelected?.Invoke(this);
    }

    /// <summary>
    /// Mark tile as correctly selected.
    /// </summary>
    public void MarkCorrect() {
        isClickable = false;
        backgroundImage.color = Color.green;
    }

    /// <summary>
    /// Mark tile as wrongly selected.
    /// </summary>
    public void MarkWrong() {
        isClickable = false;
        backgroundImage.color = Color.red;
    }
}
