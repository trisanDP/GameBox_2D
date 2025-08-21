using UnityEngine;
using System.Collections;

public class CustomGameNotification : MonoBehaviour
{
    public GameObject notificationObject; // Assign in Inspector
    public float duration = 3f;
    // Call this method to show the notification
    public void ShowNotification() {
        StopAllCoroutines(); // Stop any existing notification coroutine
        StartCoroutine(ShowNotificationCoroutine(duration));
    }

    private IEnumerator ShowNotificationCoroutine(float duration) {
        notificationObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        notificationObject.SetActive(false);
    }
}
