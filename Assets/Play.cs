using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play : MonoBehaviour {

    public static bool bubbleVisible = false;
    public static string bubbleRay_Method = "hand centered";

    GameObject controllerLeft;
    GameObject controllerRight;
    GameObject controller;
    GameObject ray;
    GameObject[] playProps;
    GameObject bubbleRay_BubbleBall;
    GameObject bubbleRay_BubbleCircle;

    GameObject selectedObject;

    // Use this for initialization
    void Start () {
        controllerLeft = GameObject.Find("VROrigin/[CameraRig]/Controller (left)");
        controllerRight = GameObject.Find("VROrigin/[CameraRig]/Controller (right)");
        controller = controllerRight;
        ray = GameObject.Find("Ray");
        playProps = GameObject.FindGameObjectsWithTag("play prop");
        bubbleRay_BubbleBall = GameObject.Find("Bubble Ray/Bubble Ball");
        bubbleRay_BubbleCircle = GameObject.Find("Bubble Ray/Bubble Circle");
        ray.transform.SetParent(controller.transform);
    }
	
	// Update is called once per frame
	void Update () {
        BubbleRay();
    }

    void BubbleRay()
    {
        SelectObject(selectedObject, 0);
        Vector3 p = controller.transform.position;
        Vector3 v = QuaternionToVector(controller.transform.rotation);
        float minDist = 1e20f;
        switch (bubbleRay_Method) {
            case "hand centered":
                Vector3 intersectPoint = new Vector3(0, 0, -5);
                foreach (GameObject g in playProps) {
                    Vector3 q = g.transform.position;
                    float t = -(p.x + p.y + p.z - q.x - q.y - q.z) / (v.x + v.y + v.z);
                    Vector3 i = p + v * t;
                    float d = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (d < minDist) {
                        minDist = d;
                        selectedObject = g;
                        intersectPoint = i;
                    }
                }
                Vector3 renderPoint = (bubbleVisible) ? intersectPoint : new Vector3(0, 0, -5);
                float renderScale = (bubbleVisible) ? minDist * 2 : 1;
                bubbleRay_BubbleBall.transform.position = renderPoint;
                bubbleRay_BubbleBall.transform.localScale = new Vector3(1, 1, 1) * renderScale;
                break;
                
            case "plane view":
                Vector3 pointInDepth = new Vector3(0, 0, -5);
                foreach (GameObject g in playProps) {
                    Vector3 q = g.transform.position;
                    float t = (q.z - p.z) / v.z;
                    Vector3 i = p + v * t;
                    float d = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (d < minDist) {
                        minDist = d;
                        selectedObject = g;
                        pointInDepth = i;
                    }
                }
                renderPoint = (bubbleVisible) ? pointInDepth : new Vector3(0, 0, -5);
                renderScale = (bubbleVisible) ? minDist * 2 : 1;
                bubbleRay_BubbleBall.transform.position = renderPoint;
                bubbleRay_BubbleBall.transform.localScale = new Vector3(1, 1, 0.01f) * renderScale;
                break;
        }
        SelectObject(selectedObject, 1);
    }

    void SelectObject(GameObject g, int status) {
        if (g == null) return;
        if (status == 0) {
            g.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
        } else {
            g.GetComponent<Renderer>().material.color = new Color(0, 1, 1);
        }
    }

    Vector3 QuaternionToVector(Quaternion q) {
        Vector3 r = q.eulerAngles / 180.0f * Mathf.Acos(-1);
        float dx = Mathf.Cos(r.x) * Mathf.Sin(r.y);
        float dy = -Mathf.Sin(r.x);
        float dz = Mathf.Cos(r.x) * Mathf.Cos(r.y);
        return new Vector3(dx, dy, dz);
    }
}
