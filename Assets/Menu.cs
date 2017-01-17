using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    GameObject button_BubbleRay_BubbleVisible;
    GameObject button_BubbleRay_HandCentered;
    GameObject button_BubbleRay_PlaneView;

    // Use this for initialization
    void Start() {
        button_BubbleRay_BubbleVisible = GameObject.Find("Menu/Button Bubble Visible");
        button_BubbleRay_HandCentered = GameObject.Find("Menu/Bubble Ray/Button Hand Centered");
        button_BubbleRay_PlaneView = GameObject.Find("Menu/Bubble Ray/Button Plane View");
    }

    // Update is called once per frame
    void Update() {
    }

    void ClearButtonVisibility() {
        button_BubbleRay_HandCentered.SetActive(false);
        button_BubbleRay_PlaneView.SetActive(false);
    }

    public void OnClick_BubbleRay() {
        ClearButtonVisibility();
        button_BubbleRay_HandCentered.SetActive(true);
        button_BubbleRay_PlaneView.SetActive(true);
    }

    public void OnClick_BubbleRay_BubbleVisible() {
        bool visible = Play.bubbleVisible;
        Text text = button_BubbleRay_BubbleVisible.transform.GetChild(0).GetComponent<Text>();
        if (visible == true) {
            Play.bubbleVisible = false;
            text.text = "Bubble Visible";
        } else {
            Play.bubbleVisible = true;
            text.text = "Bubble Invisible";
        }
    }

    public void OnClick_BubbleRay_HandCentered() {
        Play.bubbleRay_Method = "hand centered";
    }

    public void OnClick_BubbleRay_PlaneView() {
        Play.bubbleRay_Method = "plane view";
    }
}
