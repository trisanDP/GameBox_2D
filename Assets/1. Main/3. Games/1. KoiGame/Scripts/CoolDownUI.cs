// CooldownUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CooldownUI : MonoBehaviour {
    [SerializeField] private Slider cooldownSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color cooldownColor = Color.gray;

    private Coroutine fillRoutine;

    public void StartCooldown(float duration) {
        if(fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(FillCooldown(duration));
    }

    private IEnumerator FillCooldown(float duration) {
        cooldownSlider.value = 0;
        fillImage.color = cooldownColor;

        float t = 0;
        while(t < duration) {
            t += Time.deltaTime;
            cooldownSlider.value = Mathf.Clamp01(t / duration);
            yield return null;
        }

        fillImage.color = readyColor;
        cooldownSlider.value = 1;
    }
}
