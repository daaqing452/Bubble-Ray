using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play : MonoBehaviour {

    public static bool bubbleRayBubbleVisible = false;

    GameObject controllerLeft;
    GameObject controllerRight;
    GameObject controller;
    GameObject[] playProps;
    GameObject bubble;
    GameObject ray;

    // Use this for initialization
    void Start () {
        controllerLeft = GameObject.Find("VROrigin/[CameraRig]/Controller (left)");
        controllerRight = GameObject.Find("VROrigin/[CameraRig]/Controller (right)");
        controller = controllerRight;
        playProps = GameObject.FindGameObjectsWithTag("PlayProp");
        bubble = GameObject.Find("Bubble Ray/Bubble");
        ray = GameObject.Find("Bubble Ray/Ray");
        ray.transform.SetParent(controller.transform);
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 p = controller.transform.position;
        Vector3 v = QuaternionToVector(controller.transform.rotation);
        // indicator.transform.position = p + v * 5;
        GameObject selectObject = null;
        float minDist = 1e20f;
        Vector3 intersectionPoint = new Vector3(0, 0, -5);
        foreach (GameObject g in playProps) {
            g.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            Vector3 q = g.transform.position;
            float t = -(p.x + p.y + p.z - q.x - q.y - q.z) / (v.x + v.y + v.z);
            Vector3 i = p + v * t;
            float d = (q - i).magnitude - g.transform.localScale.x / 2;
            if (d < minDist) {
                minDist = d;
                selectObject = g;
                intersectionPoint = i;
            }
        }
        if (bubbleRayBubbleVisible) {
            bubble.transform.position = intersectionPoint;
            bubble.transform.localScale = new Vector3(1, 1, 1) * minDist * 2;
        } else {
            bubble.transform.position = new Vector3(0, 0, -5);
            bubble.transform.localScale = new Vector3(1, 1, 1);
        }
        selectObject.GetComponent<Renderer>().material.color = new Color(0, 1, 1);
    }

    Vector3 QuaternionToVector(Quaternion q) {
        Vector3 r = q.eulerAngles / 180.0f * Mathf.Acos(-1);
        float dx = Mathf.Cos(r.x) * Mathf.Sin(r.y);
        float dy = -Mathf.Sin(r.x);
        float dz = Mathf.Cos(r.x) * Mathf.Cos(r.y);
        return new Vector3(dx, dy, dz);
    }
}
