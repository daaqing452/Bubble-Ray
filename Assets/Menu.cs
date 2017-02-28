﻿using System.Collections;
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
    GameObject button_BubbleRay_HandCentered;
    GameObject button_BubbleRay_DepthPlane;
    GameObject button_BubbleRay_FixedPlane;
    GameObject button_BubbleRay_CentripetalPlane;
    GameObject button_BubbleRay_Angular;
    GameObject button_BubbleRay_DepthSphere;
    GameObject button_BubbleRay_FixedSphere;
    
    //  feedback
    GameObject button_Cue_Ray;
    GameObject button_Cue_FishPole;
    GameObject button_Cue_Bubble;

    //  main
    GameObject button_Start;

    void Start() {
        //  static gameobject
        text_Info = GameObject.Find("Menu/Info");
        button_BubbleRay_HandCentered = GameObject.Find("Menu/Bubble Ray/Button Hand Centered");
        button_BubbleRay_DepthPlane = GameObject.Find("Menu/Bubble Ray/Button Depth Plane");
        button_BubbleRay_FixedPlane = GameObject.Find("Menu/Bubble Ray/Button Fixed Plane");
        button_BubbleRay_CentripetalPlane = GameObject.Find("Menu/Bubble Ray/Button Centripetal Plane");
        button_BubbleRay_Angular = GameObject.Find("Menu/Bubble Ray/Button Angular");
        button_BubbleRay_DepthSphere = GameObject.Find("Menu/Bubble Ray/Button Depth Sphere");
        button_BubbleRay_FixedSphere = GameObject.Find("Menu/Bubble Ray/Button Fixed Sphere");
        subbutton_BubbleRay = GameObject.FindGameObjectsWithTag("subbutton bubble ray");
        button_Cue_Ray = GameObject.Find("Menu/Cue/Button Ray");
        button_Cue_FishPole = GameObject.Find("Menu/Cue/Button Fish Pole");
        button_Cue_Bubble = GameObject.Find("Menu/Cue/Button Bubble");
        button_Start = GameObject.Find("Menu/Start");

        //  initiate
        OnClick_BubbleRay_HandCentered();
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

    public void OnClick_BubbleRay_HandCentered() {
        BubbleRay.method = "hand centered";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_HandCentered, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Hand Centered";
    }
    public void OnClick_BubbleRay_DepthPlane() {
        BubbleRay.method = "depth plane";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_DepthPlane, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Depth Plane";
    }
    public void OnClick_BubbleRay_FixedPlane() {
        BubbleRay.method = "fixed plane";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_FixedPlane, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Fixed Plane";
    }
    public void OnClick_BubbleRay_CentripetalPlane() {
        BubbleRay.method = "centripetal plane";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_CentripetalPlane, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Centripetal Plane";
    }
    public void OnClick_BuubleRay_Angular() {
        BubbleRay.method = "angular";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_Angular, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Angular";
    }
    public void OnClick_BuubleRay_DepthSphere() {
        BubbleRay.method = "depth sphere";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_DepthSphere, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Depth Sphere";
    }
    public void OnClick_BuubleRay_FixedSphere() {
        BubbleRay.method = "fixed sphere";
        ClearTriggeredButton(subbutton_BubbleRay);
        SetButtonColor(button_BubbleRay_FixedSphere, COLOR_TRIGGER);
        text_Info.GetComponent<Text>().text = "Bubble Ray - Fixed Sphere";
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
