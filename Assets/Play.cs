using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using HTC.UnityPlugin.PoseTracker;
using HTC.UnityPlugin.Vive;
using System;

public class Play : MonoBehaviour {
    //  menu configuration
    public static bool visibilityRay = false;

    //  constant
    public const string TAG_PLAY_PROP = "play prop";

    //  static gameobject
    public GameObject[] playProps;
    public GameObject cameraHead;
    public GameObject controller;
    GameObject controllerLeft;
    GameObject controllerRight;
    GameObject ray;
    HandRole handRole;
    public GameObject signExperimentCompleted;

    //  method
    GameObject selectedObject;
    Experiment experiment;
    Technique technique;
    BubbleRay bubbleRay;
    
    void Start() {
        //  static gameobject
        cameraHead = GameObject.Find("VROrigin/[CameraRig]/Camera (eye)");
        controllerLeft = GameObject.Find("VROrigin/[CameraRig]/Controller (left)");
        controllerRight = GameObject.Find("VROrigin/[CameraRig]/Controller (right)");
        controller = controllerRight;
        ray = GameObject.Find("Ray");
        ray.transform.SetParent(controller.transform);
        handRole = HandRole.RightHand;
        signExperimentCompleted = GameObject.Find("Experiment Completed");

        //  initiate methods
        bubbleRay = new BubbleRay(this);
        technique = bubbleRay;
        experiment = new Experiment(this, "example.conf");
    }
    
	void Update() {
        //  find current selected object
        selectedObject = technique.Select();

        //  accumulate movement
        experiment.ControllerMove(controller.transform.position);

        //  user event
        if (ViveInput.GetPress(handRole, ControllerButton.FullTrigger)) {
            experiment.Select(selectedObject);
        }

        //  color
        foreach (GameObject g in playProps) {
            if (g == selectedObject) {
                g.GetComponent<Renderer>().material = Resources.Load("Dark Green") as Material;
            } else if (g == experiment.targetObject) {
                g.GetComponent<Renderer>().material = Resources.Load("Dark Blue") as Material;
            } else {
                g.GetComponent<Renderer>().material.color = Color.white;
            }
        }
        
        //  feedback
        ray.SetActive(visibilityRay);
    }
    
    public void OnClick_Start() {
        experiment.Start();
    }
}

abstract class Technique {
    public abstract GameObject Select();
    
    public static Vector3 QuaternionToVector(Quaternion q) {
        Vector3 r = q.eulerAngles / 180.0f * Mathf.Acos(-1);
        float dx = Mathf.Cos(r.x) * Mathf.Sin(r.y);
        float dy = -Mathf.Sin(r.x);
        float dz = Mathf.Cos(r.x) * Mathf.Cos(r.y);
        return new Vector3(dx, dy, dz);
    }
}

class BubbleRay : Technique {
    //  menu configuration
    public static string method = "";
    public static bool visibilityBubble = false;
    public static bool visibilityFishPole = false;

    //  constant
    public float EPS = 1e-5f;
    public Vector3 POINT_HIDE = new Vector3(0, 0, -5);
    public Quaternion QUETERNION_NULL = new Quaternion(1, 0, 0, 0);
    public Vector3 UNIT_BALL = new Vector3(1, 1, 1);
    public Vector3 UNIT_CIRCLE = new Vector3(1, 1, 0.01f);

    //  work 
    Play play;
    GameObject bubble;
    LineRenderer fishPole;

    public BubbleRay(Play play) {
        this.play = play;
        bubble = GameObject.Find("Bubble Ray/Bubble");
        fishPole = GameObject.Find("Fish Pole").GetComponent<LineRenderer>();
    }

    public override GameObject Select() {
        //  source geomatric data
        Vector3 p = play.controller.transform.position;
        Vector3 v = QuaternionToVector(play.controller.transform.rotation);
        Vector3 e = play.cameraHead.transform.position;

        //  initiate
        GameObject selectedObject = null;
        Vector3 renderPoint = POINT_HIDE;
        Quaternion renderRotation = QUETERNION_NULL;
        Vector3 renderUnit = UNIT_CIRCLE;
        float range0 = 1e20f;
        float range1 = 1e20f;

        float minF = 1e20f;
        switch (method) {
            case "hand centered":
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    if (Mathf.Abs(v.x + v.y + v.z) < EPS) continue;
                    float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (v.x * v.x + v.y * v.y + v.z * v.z);
                    Vector3 i = p + v * t;
                    float f = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (f < minF) {
                        renderPoint = i;
                        range1 = range0;
                        range0 = f;
                        minF = f;
                        selectedObject = g;
                    } else {
                        range1 = Mathf.Min(range1, f);
                    }
                }
                renderUnit = UNIT_BALL;
                break;

            case "depth plane":
                foreach (GameObject g in play.playProps) {
                    if (Mathf.Abs(v.z) < EPS) continue;
                    Vector3 q = g.transform.position;
                    float t = (q.z - p.z) / v.z;
                    Vector3 i = p + v * t;
                    float f = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (f < minF) {
                        renderPoint = i;
                        range1 = range0;
                        range0 = f;
                        minF = f;
                        selectedObject = g;
                    } else {
                        range1 = Mathf.Min(range1, f);
                    }
                }
                break;

            case "fixed plane":
                float D = -1e20f;
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    D = Mathf.Max(D, q.z + g.transform.localScale.x / 2);
                }
                foreach (GameObject g in play.playProps) {
                    if (Mathf.Abs(v.z) < EPS) continue;
                    Vector3 q = g.transform.position;
                    Vector3 qq = e + (q - e) / (q.z - e.z) * D;
                    float t = (D - p.z) / v.z;
                    Vector3 i = p + v * t;
                    float ShadowScale = g.transform.localScale.x / (q.z - e.z) * D;
                    float f = (qq - i).magnitude - ShadowScale / 2;
                    if (f < minF) {
                        renderPoint = i;
                        range1 = range0;
                        range0 = f;
                        minF = f;
                        selectedObject = g;
                    }
                    else {
                        range1 = Mathf.Min(range1, f);
                    }
                }
                break;

            case "centripetal plane":
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    Vector3 n = QuaternionToVector(play.cameraHead.transform.rotation);
                    if (Mathf.Abs(v.x * n.x + v.y * n.y + v.z * n.z) < EPS) continue;
                    float t = -((p.x - q.x) * n.x + (p.y - q.y) * n.y + (p.z - q.z) * n.z) / (v.x * n.x + v.y * n.y + v.z * n.z);
                    Vector3 i = p + v * t;
                    float f = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (f < minF) {
                        renderPoint = i;
                        range1 = range0;
                        range0 = f;
                        minF = f;
                        selectedObject = g;
                    } else {
                        range1 = Mathf.Min(range1, f);
                    }
                }
                renderRotation = play.cameraHead.transform.rotation;
                break;

            case "depth sphere":
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    Vector3 u = q - e;
                    float a = v.x * v.x + v.y * v.y + v.z * v.z;
                    float b = 2 * ((p.x - e.x) * v.x + (p.y - e.y) * v.y + (p.z - e.z) * v.z);
                    float c = (p.x - e.x) * (p.x - e.x) + (p.y - e.y) * (p.y - e.y) + (p.z - e.z) * (p.z - e.z) - u.magnitude * u.magnitude;
                    if (b * b - 4 * a * c < 0) continue;
                    if (Mathf.Abs(a) < EPS) continue;
                    float t = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
                    Vector3 i = p + v * t;
                    Vector3 w = i - e;
                    float alpha = Mathf.Acos((u.x * w.x + u.y * w.y + u.z * w.z) / u.magnitude / w.magnitude);
                    float d = (q - i).magnitude - g.transform.localScale.x / 2;
                    float f = alpha * u.magnitude;
                    if (f < minF) {
                        renderPoint = i;
                        range1 = range0;
                        range0 = d;
                        minF = f;
                        selectedObject = g;
                    }
                    else {
                        range1 = Mathf.Min(range1, d);
                    }
                }
                renderRotation = play.cameraHead.transform.rotation;
                break;

            case "fixed sphere":
                float R = -1e20f;
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    R = Mathf.Max(R, (q - e).magnitude + g.transform.localScale.x / 2);
                }
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    Vector3 qq = e + (q - e).normalized * R;
                    Vector3 u = qq - e;
                    float a = v.x * v.x + v.y * v.y + v.z * v.z;
                    float b = 2 * ((p.x - e.x) * v.x + (p.y - e.y) * v.y + (p.z - e.z) * v.z);
                    float c = (p.x - e.x) * (p.x - e.x) + (p.y - e.y) * (p.y - e.y) + (p.z - e.z) * (p.z - e.z) - u.magnitude * u.magnitude;
                    if (b * b - 4 * a * c < 0) continue;
                    if (Mathf.Abs(a) < EPS) continue;
                    float t = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
                    Vector3 i = p + v * t;
                    Vector3 w = i - e;
                    float alpha = Mathf.Acos((u.x * w.x + u.y * w.y + u.z * w.z) / u.magnitude / w.magnitude);
                    float shadowScale = g.transform.localScale.x / (q - e).magnitude * R;
                    float d = (qq - i).magnitude - shadowScale / 2;
                    float f = alpha * u.magnitude;
                    if (f < minF) {
                        renderPoint = i;
                        range1 = range0;
                        range0 = d;
                        minF = f;
                        selectedObject = g;
                    }
                    else {
                        range1 = Mathf.Min(range1, d);
                    }
                }
                renderRotation = play.cameraHead.transform.rotation;
                break;

            case "angular":
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    Vector3 u = q - e;
                    if (Mathf.Abs(v.x * u.x + v.y * u.y + v.z * u.z) < EPS) continue;
                    if (Mathf.Abs(u.magnitude) < EPS) continue;
                    float t = -((p.x - q.x) * u.x + (p.y - q.y) * u.y + (p.z - q.z) * u.z) / (v.x * u.x + v.y * u.y + v.z * u.z);
                    Vector3 i = p + v * t;
                    float d = (q - i).magnitude - g.transform.localScale.x / 2;
                    float f = d / u.magnitude;
                    if (f < minF) {
                        renderPoint = i;
                        range1 = range0;
                        range0 = d;
                        minF = f;
                        selectedObject = g;
                    }
                    else {
                        range1 = Mathf.Min(range1, d);
                    }
                }
                renderRotation = play.cameraHead.transform.rotation;
                break;
        }

        //  draw bubble
        float renderScale = range0 * 2;
        //float renderScale = Mathf.Min(range0 + selectedObject.transform.localScale.x, range1) * 2;
        Transform transformBubble = bubble.transform;
        transformBubble.position = (visibilityBubble) ? renderPoint : POINT_HIDE;
        transformBubble.rotation = renderRotation;
        transformBubble.localScale = renderUnit * ((visibilityBubble) ? renderScale : 1);

        //  draw fishpole
        if (visibilityFishPole) {
            DrawTwoOrderBezierCurve(play.controller.transform.position, selectedObject.transform.position, v);
        }
        else {
            fishPole.positionCount = 0;
        }
        
        return selectedObject;
    }

    void DrawTwoOrderBezierCurve(Vector3 p, Vector3 q, Vector3 v) {
        float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (v.x * v.x + v.y * v.y + v.z * v.z);
        Vector3 r = p + v * t * 0.8f;
        List<Vector3> bs = new List<Vector3>();
        for (int i = 0; i <= 100; i++) {
            float j = i / 100.0f;
            Vector3 b = (1 - j) * (1 - j) * p + 2 * j * (1 - j) * r + j * j * q;
            bs.Add(b);
        }
        fishPole.SetPositions(bs.ToArray());
        fishPole.positionCount = bs.Count;
    }
}

class Experiment {
    //  main
    public GameObject targetObject;
    public bool completed = false;
    string fileName;
    Play play;
    bool started = false;
    List<string> record = new List<string>();

    //  measures
    int trials = 0;
    int trialsMax;
    int trialsError = 0;
    float movementTotal = 0;

    Vector3 prevPosition = Vector3.zero;

    public Experiment(Play play, string fileName) {
        this.fileName = fileName;
        this.play = play;
        Load("Configure/" + fileName);
        Next();
    }

    public void Start() {
        started = true;
        record.Add("start " + new DateTime().ToFileTime());
    }
    public void Select(GameObject selectedObject) {
        if (!started) {
            if (selectedObject == targetObject) Next();
            return;
        }
        bool correct = (selectedObject == targetObject);
        if (correct) {
            trials += 1;
            if (trials > trialsMax) { Complete(); return; }
            Next();
        }
        else {
            trialsError++;
        }
        record.Add("select " + targetObject.name + " " + correct + " " + movementTotal + " " + new DateTime().ToFileTime());
    }
    public void ControllerMove(Vector3 nowPosition) {
        if (started) {
            movementTotal += (nowPosition - prevPosition).magnitude;
        }
        prevPosition = nowPosition;
    }
    void Load(string fileName) {
        //  clean up
        play.signExperimentCompleted.SetActive(false);
        GameObject[] destroyObjects = GameObject.FindGameObjectsWithTag(Play.TAG_PLAY_PROP);
        foreach (GameObject g in destroyObjects) UnityEngine.Object.Destroy(g);
        record.Add("task " + fileName);

        //  read
        int lineNo = -1;
        GameObject newGameObject = null;
        StreamReader reader = new StreamReader(new FileStream(fileName, FileMode.Open));
        while (true) {
            lineNo++;
            string line = reader.ReadLine();
            if (line == null) break;
            string[] arr = line.Split(' ');
            switch (arr[0]) {
                case "trials":
                    trialsMax = int.Parse(arr[1]);
                    break;
                case "object":
                    PrimitiveType primitiveType = 0;
                    if (arr[1] == "sphere") primitiveType = PrimitiveType.Sphere;
                    newGameObject = GameObject.CreatePrimitive(primitiveType);
                    newGameObject.transform.parent = GameObject.Find("Play").transform;
                    newGameObject.tag = Play.TAG_PLAY_PROP;
                    break;
                case "name":
                    newGameObject.name = line.Substring(5);
                    break;
                case "position":
                    float px = float.Parse(arr[1]), py = float.Parse(arr[2]), pz = float.Parse(arr[3]);
                    newGameObject.transform.position = new Vector3(px, py, pz);
                    break;
                case "rotation":
                    float rx = float.Parse(arr[1]), ry = float.Parse(arr[2]), rz = float.Parse(arr[3]);
                    newGameObject.transform.rotation = new Quaternion(rx, ry, rz, 1);
                    break;
                case "scale":
                    float sx = float.Parse(arr[1]), sy = float.Parse(arr[2]), sz = float.Parse(arr[3]);
                    newGameObject.transform.localScale = new Vector3(sx, sy, sz);
                    break;
                case "color":
                    float cr = float.Parse(arr[1]), cg = float.Parse(arr[2]), cb = float.Parse(arr[3]);
                    newGameObject.GetComponent<Renderer>().material.color = new Color(cr, cg, cb);
                    break;
                case "end":
                    newGameObject = null;
                    break;
            }
        }
        reader.Close();

        //  reload play props
        play.playProps = GameObject.FindGameObjectsWithTag(Play.TAG_PLAY_PROP);
    }
    void Next() {
        GameObject prevTargetObject = targetObject;
        while (true) {
            int x = new System.Random().Next() % play.playProps.Length;
            targetObject = play.playProps[x];
            if (targetObject != prevTargetObject) break;
        }
    }
    void Complete() {
        completed = true;
        play.signExperimentCompleted.SetActive(true);
    }
}