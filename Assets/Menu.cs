using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    //  color
    Color COLOR_TRIGGER = new Color(0.8f, 1, 0.8f);
    Color COLOR_NONE = new Color(1, 1, 1);
    
    //  text info
    GameObject text_Info;

    //  bubble ray type
    GameObject[] subbutton_BubbleRay;
    GameObject button_BubbleRay_PointingBack;
    GameObject button_BubbleRay_HandDistance;
    GameObject button_BubbleRay_HandAngular;
    GameObject button_BubbleRay_EyesDistance;
    GameObject button_BubbleRay_EyesAngular;
    
    //  feedback
    GameObject button_Cue_Ray;
    GameObject button_Cue_FishPole;
    GameObject button_Cue_Bubble;

    void Start() {
        //  static gameobject
        text_Info = GameObject.Find("Menu/Info");
        button_BubbleRay_PointingBack = GameObject.Find("Menu/Bubble Ray/Button Pointing Back");
        button_BubbleRay_HandDistance = GameObject.Find("Menu/Bubble Ray/Button Hand Distance");
        button_BubbleRay_HandAngular = GameObject.Find("Menu/Bubble Ray/Button Hand Angular");
        button_BubbleRay_EyesDistance = GameObject.Find("Menu/Bubble Ray/Button Eyes Distance");
        button_BubbleRay_EyesAngular = GameObject.Find("Menu/Bubble Ray/Button Eyes Angular");
        subbutton_BubbleRay = GameObject.FindGameObjectsWithTag("subbutton bubble ray");
        button_Cue_Ray = GameObject.Find("Menu/Cue/Button Ray");
        button_Cue_FishPole = GameObject.Find("Menu/Cue/Button Fish Pole");
        button_Cue_Bubble = GameObject.Find("Menu/Cue/Button Bubble");

        //  initiate
        OnClick_BubbleRay_HandDistance();
        OnClick_Cue_Ray();
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

    public void OnClick_BubbleRay_PointingBack() {
        BubbleRay.method = "PointingBack";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_PointingBack, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Pointing Back";
    }
    public void OnClick_BubbleRay_HandDistance() {
        BubbleRay.method = "HandDistance";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_HandDistance, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Hand Distance";
    }
    public void OnClick_BubbleRay_HandAngular() {
        BubbleRay.method = "HandAngular";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_HandAngular, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Hand Angular";
    }
    public void OnClick_BubbleRay_EyesDistance() {
        BubbleRay.method = "EyesDistance";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_EyesDistance, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Eyes Distance";
    }
    public void OnClick_BuubleRay_EyesAngular() {
        BubbleRay.method = "EyesAngular";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_EyesAngular, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Eyes Angular";
    }

    public void OnClick_Cue_Ray() {
        if (Play.visibilityRay) {
            Play.visibilityRay = false;
            SetButtonColor(button_Cue_Ray, COLOR_NONE);
        } else {
            Play.visibilityRay = true;
            SetButtonColor(button_Cue_Ray, COLOR_TRIGGER);
        }
    }
    public void OnClick_Cue_FishPole() {
        if (BubbleRay.visibilityFishPole) {
            BubbleRay.visibilityFishPole = false;
            SetButtonColor(button_Cue_FishPole, COLOR_NONE);
        } else {
            BubbleRay.visibilityFishPole = true;
            SetButtonColor(button_Cue_FishPole, COLOR_TRIGGER);
        }
    }
    public void OnClick_Cue_Bubble() {
        if (BubbleRay.visibilityBubble) {
            BubbleRay.visibilityBubble = false;
            SetButtonColor(button_Cue_Bubble, COLOR_NONE);
        } else {
            BubbleRay.visibilityBubble = true;
            SetButtonColor(button_Cue_Bubble, COLOR_TRIGGER);
        }
    }
}
