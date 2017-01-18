using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    Color COLOR_TRIGGER = new Color(0.8f, 1, 0.8f);
    Color COLOR_NONE = new Color(1, 1, 1);

    GameObject text_Info;

    GameObject button_Cue_Ray;
    GameObject button_Cue_FishPole;
    GameObject button_Cue_Bubble;

    GameObject button_BubbleRay_HandCentered;
    GameObject button_BubbleRay_DynamicPlane;
    GameObject button_BubbleRay_DynamicTangent;
    GameObject[] subbutton_BubbleRay;
    
    void Start() {
        text_Info = GameObject.Find("Menu/Info");
        button_Cue_Ray = GameObject.Find("Menu/Cue/Button Ray");
        button_Cue_FishPole = GameObject.Find("Menu/Cue/Button Fish Pole");
        button_Cue_Bubble = GameObject.Find("Menu/Cue/Button Bubble");

        button_BubbleRay_HandCentered = GameObject.Find("Menu/Bubble Ray/Button Hand Centered");
        button_BubbleRay_DynamicPlane = GameObject.Find("Menu/Bubble Ray/Button Dynamic Plane");
        button_BubbleRay_DynamicTangent = GameObject.Find("Menu/Bubble Ray/Button Dynamic Tangent");
        subbutton_BubbleRay = GameObject.FindGameObjectsWithTag("subbutton bubble ray");

        OnClick_BubbleRay_HandCentered();
    }
    
    void Update() {
    }
    
    void SetButtonColor(GameObject g, Color c) {
        g.GetComponent<Image>().color = c;
    }
    void ClearTriggeredButton(GameObject[] group) {
        foreach (GameObject g in group) {
            SetButtonColor(g, COLOR_NONE);
        }
    }

    public void OnClick_BubbleRay_HandCentered() {
        BubbleRay.method = "hand centered";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_HandCentered, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Hand Centered";
    }
    public void OnClick_BubbleRay_DynamicPlane() {
        BubbleRay.method = "dynamic plane";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_DynamicPlane, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Plane View";
    }
    public void OnClick_BubbleRay_DynamicTangent() {
        BubbleRay.method = "dynamic tangent";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_DynamicTangent, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Sphere View";
    }

    public void OnClick_Cue_Ray() {
        if (Play.visibilityRay) {
            Play.visibilityRay = false;
            SetButtonColor(button_Cue_Ray, COLOR_NONE);
        } else {
            Play.visibilityRay = true;
            Debug.Log("enter");
            SetButtonColor(button_Cue_Ray, COLOR_TRIGGER);
        }
    }
    public void OnClick_Cue_FishPole() {
        if (Play.visibilityFishPole) {
            Play.visibilityFishPole = false;
            SetButtonColor(button_Cue_FishPole, COLOR_NONE);
        } else {
            Play.visibilityFishPole = true;
            SetButtonColor(button_Cue_FishPole, COLOR_TRIGGER);
        }
    }
    public void OnClick_Cue_Bubble() {
        Debug.Log(BubbleRay.visibilityBubble);
        if (BubbleRay.visibilityBubble) {
            BubbleRay.visibilityBubble = false;
            SetButtonColor(button_Cue_Bubble, COLOR_NONE);
        } else {
            BubbleRay.visibilityBubble = true;
            SetButtonColor(button_Cue_Bubble, COLOR_TRIGGER);
        }
    }
}
