using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class VisualsSettingsUI : MonoBehaviour
{
    public List<Image> bgs;
    public Button VisualBTN;

    public bool DarkMode;
    public void Start() {
        VisualBTN.onClick.AddListener(OnBTNPressed);

    }

    public void OnBTNPressed() {
        if(!DarkMode) {
            foreach(Image bg in bgs) {
                bg.color = Color.black;
            }

            DarkMode = true;
            Debug.Log("Dark");
        } else {
            foreach(Image bg in bgs) {
                bg.color = Color.white;
            }

            DarkMode = false;
            Debug.Log("White");
        }

    }
}
