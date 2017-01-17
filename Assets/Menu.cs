using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    GameObject button_BubbleRay_BubbleVisible;
    GameObject button_BubbleRay_Center;

    // Use this for initialization
    void Start() {
        button_BubbleRay_BubbleVisible = GameObject.Find("Menu/Bubble Ray/Button Bubble Visible");
        button_BubbleRay_Center = GameObject.Find("Menu/Bubble Ray/Button Center");
    }

    // Update is called once per frame
    void Update() {
    }

    void ClearButtonVisibility() {
        button_BubbleRay_BubbleVisible.SetActive(false);
        button_BubbleRay_Center.SetActive(false);
    }

    public void OnClick_BubbleRay() {
        ClearButtonVisibility();
        button_BubbleRay_BubbleVisible.SetActive(true);
        button_BubbleRay_Center.SetActive(true);
    }

    public void OnClick_BubbleRay_BubbleVisible() {
        bool visible = Play.bubbleRayBubbleVisible;
        Text text = button_BubbleRay_BubbleVisible.transform.GetChild(0).GetComponent<Text>();
        if (visible == true) {
            Play.bubbleRayBubbleVisible = false;
            text.text = "Bubble Visible";
        } else {
            Play.bubbleRayBubbleVisible = true;
            text.text = "Bubble Invisible";
        }
    }

    public void OnClick_BubbleRay_Center() {

    }
}
