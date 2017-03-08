using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    //  color
    Color COLOR_TRIGGER = new Color(0.8f, 1, 0.8f);
    Color COLOR_NONE = new Color(1, 1, 1);

    //  play
    Play play;

    //  bubble ray type
    GameObject[] subbutton_BubbleRay;
    GameObject button_BubbleRay_BackPlane;
    GameObject button_BubbleRay_BackSphere;
    GameObject button_BubbleRay_HandDistance;
    GameObject button_BubbleRay_HandAngular;
    GameObject button_BubbleRay_Test;

    //  feedback
    GameObject button_Cue_FishPole;
    GameObject button_Cue_Bubble;

    //  technique
    GameObject[] subbutton_Technique;
    GameObject button_Technique_NaiveRay;
    GameObject button_Technique_BubbleRay;

    void Start() {
        play = GameObject.Find("Play").GetComponent<Play>();

        //  static gameobject
        button_BubbleRay_BackPlane = GameObject.Find("Menu/Bubble Ray/Button Back Plane");
        button_BubbleRay_BackSphere = GameObject.Find("Menu/Bubble Ray/Button Back Sphere");
        button_BubbleRay_HandDistance = GameObject.Find("Menu/Bubble Ray/Button Hand Distance");
        button_BubbleRay_HandAngular = GameObject.Find("Menu/Bubble Ray/Button Hand Angular");
        button_BubbleRay_Test = GameObject.Find("Menu/Bubble Ray/Button Test");
        subbutton_BubbleRay = GameObject.FindGameObjectsWithTag("subbutton bubble ray");
        
        button_Cue_FishPole = GameObject.Find("Menu/Cue/Button Fish Pole");
        button_Cue_Bubble = GameObject.Find("Menu/Cue/Button Bubble");

        button_Technique_NaiveRay = GameObject.Find("Menu/Technique/Button Naive Ray");
        button_Technique_BubbleRay = GameObject.Find("Menu/Technique/Button Bubble Ray");
        subbutton_Technique = GameObject.FindGameObjectsWithTag("subbutton technique");

        //  initiate
        OnClick_Technique_BubbleRay();
        OnClick_BubbleRay_HandDistance();
        OnClick_Cue_FishPole();
        OnClick_Cue_Bubble();
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

    public void OnClick_BubbleRay_BackPlane() {
        play.technique.method = "BackPlane";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_BackPlane, COLOR_TRIGGER);
    }
    public void OnClick_BubbleRay_BackSphere() {
        play.technique.method = "BackSphere";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_BackSphere, COLOR_TRIGGER);
    }
    public void OnClick_BubbleRay_HandDistance() {
        play.technique.method = "HandDistance";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_HandDistance, COLOR_TRIGGER);
    }
    public void OnClick_BubbleRay_HandAngular() {
        play.technique.method = "HandAngular";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_HandAngular, COLOR_TRIGGER);
    }
    public void OnClick_BubbleRay_Test() {
        play.technique.method = "EyesAngular";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_Test, COLOR_TRIGGER);
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

    public void OnClick_Technique_NaiveRay() {
        play.ChangeTechnique<NaiveRay>();
        ClearTriggeredButton(subbutton_Technique);
        SetButtonColor(button_Technique_NaiveRay, COLOR_TRIGGER);
    }
    public void OnClick_Technique_BubbleRay() {
        play.ChangeTechnique<BubbleRay>();
        ClearTriggeredButton(subbutton_Technique);
        SetButtonColor(button_Technique_BubbleRay, COLOR_TRIGGER);
    }
}
