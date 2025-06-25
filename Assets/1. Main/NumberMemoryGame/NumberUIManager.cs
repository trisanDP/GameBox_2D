// UIManager.cs
using System.Collections;
using UnityEngine;
using TMPro;


public class NumberUIManager : MonoBehaviour {
    [Header("UI Elements")]
    public TMP_Text levelText;
    public TMP_Text feedbackText;

    public void SetLevel(int level) {
        levelText.text = "Level: " + level;
    }

    public void ShowFeedback(string message) {
        StopAllCoroutines();
        StartCoroutine(ShowAndHide(message));
    }

    private IEnumerator ShowAndHide(string message) {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        feedbackText.gameObject.SetActive(false);
    }
}

