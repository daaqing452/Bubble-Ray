using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play : MonoBehaviour {

    Color COLOR_NONE = new Color(1, 1, 1);
    Color COLOR_SELECTED = new Color(0, 0, 1);

    public static bool visibilityRay = false;
    public static bool visibilityFishPole = false;

    public GameObject[] playProps;
    public GameObject cameraHead;
    public GameObject controller;
    GameObject controllerLeft;
    GameObject controllerRight;
    GameObject ray;
    LineRenderer fishPole;

    GameObject selectedObject;
    BubbleRay bubbleRay;

    void Start () {
        playProps = GameObject.FindGameObjectsWithTag("play prop");
        cameraHead = GameObject.Find("VROrigin/[CameraRig]/Camera (eye)");
        controllerLeft = GameObject.Find("VROrigin/[CameraRig]/Controller (left)");
        controllerRight = GameObject.Find("VROrigin/[CameraRig]/Controller (right)");
        controller = controllerRight;
        ray = GameObject.Find("Ray");
        ray.transform.SetParent(controller.transform);
        fishPole = GameObject.Find("Fish Pole").GetComponent<LineRenderer>();
        bubbleRay = new BubbleRay(this);
    }
	
	void Update ()
    {
        SelectObject(selectedObject, 0);
        selectedObject = bubbleRay.Select();
        SelectObject(selectedObject, 1);

        if (visibilityRay) {
            ray.SetActive(true);
        } else {
            ray.SetActive(false);
        }

        if (visibilityFishPole) {
            Vector3 v = QuaternionToVector(controller.transform.rotation);
            DrawTwoOrderBezierCurve(controller.transform.position, selectedObject.transform.position, v);
        } else {
            fishPole.numPositions = 0;
        }
    }
    
    void SelectObject(GameObject g, int status) {
        if (g == null) return;
        if (status == 0) {
            g.GetComponent<Renderer>().material.color = COLOR_NONE;
        } else {
            g.GetComponent<Renderer>().material.color = COLOR_SELECTED;
            //g.GetComponent<Renderer>().material = Resources.Load("Dark Purple.mat") as Material;
        }
    }
    void DrawTwoOrderBezierCurve(Vector3 p, Vector3 q, Vector3 v)
    {
        float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (v.x * v.x + v.y * v.y + v.z * v.z);
        Vector3 r = p + v * t * 0.8f;
        List<Vector3> bs = new List<Vector3>();
        for (int i = 0; i <= 100; i++) {
            float j = i / 100.0f;
            Vector3 b = (1 - j) * (1 - j) * p + 2 * j * (1 - j) * r + j * j * q;
            bs.Add(b);
        }
        fishPole.SetPositions(bs.ToArray());
        fishPole.numPositions = bs.Count;
    }
    public static Vector3 QuaternionToVector(Quaternion q)
    {
        Vector3 r = q.eulerAngles / 180.0f * Mathf.Acos(-1);
        float dx = Mathf.Cos(r.x) * Mathf.Sin(r.y);
        float dy = -Mathf.Sin(r.x);
        float dz = Mathf.Cos(r.x) * Mathf.Cos(r.y);
        return new Vector3(dx, dy, dz);
    }
}

class BubbleRay {
    public static string method = "";
    public static bool visibilityBubble = false;

    public float EPS = 1e-5f;
    public Vector3 POINT_HIDE = new Vector3(0, 0, -5);
    public Quaternion QUETERNION_NULL = new Quaternion(1, 0, 0, 0);
    public Vector3 UNIT_BALL = new Vector3(1, 1, 1);
    public Vector3 UNIT_CIRCLE = new Vector3(1, 1, 0.01f);

    Play play;
    GameObject bubble;

    public BubbleRay(Play play) {
        this.play = play;
        bubble = GameObject.Find("Bubble Ray/Bubble");
    }

    public GameObject Select()
    {
        Vector3 p = play.controller.transform.position;
        Vector3 v = Play.QuaternionToVector(play.controller.transform.rotation);
        GameObject selectedObject = null;
        Transform transformBubble = bubble.transform;
        float minDist = 1e20f;
        switch (method)
        {
            case "hand centered":
                Vector3 intersectPoint = POINT_HIDE;
                foreach (GameObject g in play.playProps)
                {
                    Vector3 q = g.transform.position;
                    if (Mathf.Abs(v.x + v.y + v.z) < EPS) continue;
                    float t = -(p.x + p.y + p.z - q.x - q.y - q.z) / (v.x + v.y + v.z);
                    Vector3 i = p + v * t;
                    float d = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (d < minDist)
                    {
                        minDist = d;
                        selectedObject = g;
                        intersectPoint = i;
                    }
                }
                transformBubble.position = (visibilityBubble) ? intersectPoint : POINT_HIDE;
                transformBubble.rotation = QUETERNION_NULL;
                transformBubble.localScale = UNIT_BALL * ((visibilityBubble) ? minDist * 2 : 1);
                break;

            case "dynamic plane":
                Vector3 pointInPlane = POINT_HIDE;
                foreach (GameObject g in play.playProps)
                {
                    Vector3 q = g.transform.position;
                    if (Mathf.Abs(v.z) < EPS) continue;
                    float t = (q.z - p.z) / v.z;
                    Vector3 i = p + v * t;
                    float d = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (d < minDist)
                    {
                        minDist = d;
                        selectedObject = g;
                        pointInPlane = i;
                    }
                }
                transformBubble.position = (visibilityBubble) ? pointInPlane : POINT_HIDE;
                transformBubble.rotation = QUETERNION_NULL;
                transformBubble.localScale = UNIT_CIRCLE * ((visibilityBubble) ? minDist * 2 : 1);
                break;

            case "dynamic tangent":
                Vector3 pointInSphere = POINT_HIDE;
                foreach (GameObject g in play.playProps)
                {
                    Vector3 q = g.transform.position;
                    Vector3 e = q - play.cameraHead.transform.position;
                    if (Mathf.Abs(v.x * e.x + v.y * e.y + v.z * e.z) < EPS) continue;
                    float t = -((p.x - q.x) * e.x + (p.y - q.y) * e.y + (p.z - q.z) * e.z) / (v.x * e.x + v.y * e.y + v.z * e.z);
                    Vector3 i = p + v * t;
                    float d = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (d < minDist)
                    {
                        minDist = d;
                        selectedObject = g;
                        pointInSphere = i;
                    }
                }
                transformBubble.position = (visibilityBubble) ? pointInSphere : POINT_HIDE;
                transformBubble.rotation = play.cameraHead.transform.rotation;
                transformBubble.localScale = UNIT_CIRCLE * ((visibilityBubble) ? minDist * 2 : 1);
                break;
        }
        return selectedObject;
    }
}
