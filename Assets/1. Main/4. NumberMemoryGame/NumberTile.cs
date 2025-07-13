
// NumberTile.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberTile : MonoBehaviour {
    public Image backgroundImage;
    public TMP_Text numberText;
    public Button button;

    public Color defaultColor = Color.black;
    public int Number { get; private set; }
    private System.Action<NumberTile> onSelected;
    private bool isClickable;

    public void Initialize(int number, System.Action<NumberTile> callback) {
        Number = number;
        numberText.text = number.ToString();
        onSelected = callback;
        isClickable = false;
        backgroundImage.color = defaultColor;
        numberText.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnSelected());
    }

    public void EnableInteraction(bool enabled) {
        isClickable = enabled;
        if(button != null) button.interactable = enabled;
    }

    public void HideNumber() => numberText.gameObject.SetActive(false);

    private void OnSelected() { if(isClickable) onSelected?.Invoke(this); }

    public void MarkCorrect(Color c) { isClickable = false; backgroundImage.color = c; }
    public void MarkWrong(Color c) { isClickable = false; backgroundImage.color = c; }
}

