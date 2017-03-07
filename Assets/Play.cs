﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using HTC.UnityPlugin.PoseTracker;
using HTC.UnityPlugin.Vive;

public class Play : MonoBehaviour {
    //  constant
    public const string TAG_PLAY_PROP = "play prop";

    //  game object
    public GameObject[] playProps;
    public GameObject cameraHead;
    public GameObject controller;
    GameObject controllerLeft;
    GameObject controllerRight;
    GameObject ray;
    HandRole handRole;

    GameObject signExperimentCompleted;
    GameObject signExperimentStart;
    Dropdown taskSelection;
    Text userName;
    
    AudioSource audioSelectCorrect;
    AudioSource audioSelectWrong;
    AudioSource audioStart;
    public AudioSource audioComplete;

    //  method
    GameObject selectedObject;
    Experiment experiment = null;
    public Technique technique;
    public bool visibilityRay = false;

    void Awake() {
        //  static gameobject
        cameraHead = GameObject.Find("VROrigin/[CameraRig]/Camera (eye)");
        controllerLeft = GameObject.Find("VROrigin/[CameraRig]/Controller (left)");
        controllerRight = GameObject.Find("VROrigin/[CameraRig]/Controller (right)");
        controller = controllerRight;
        ray = GameObject.Find("Ray");
        ray.transform.SetParent(controller.transform);
        handRole = HandRole.RightHand;

        signExperimentCompleted = GameObject.Find("Sign Experiment Completed");
        signExperimentStart = GameObject.Find("Sign Experiment Start");
        taskSelection = GameObject.Find("Task Selection").GetComponent<Dropdown>();
        userName = GameObject.Find("User Name Text").GetComponent<Text>();

        audioSelectCorrect = GameObject.Find("Audio/Select Correct").GetComponent<AudioSource>();
        audioSelectWrong = GameObject.Find("Audio/Select Wrong").GetComponent<AudioSource>();
        audioStart = GameObject.Find("Audio/Start").GetComponent<AudioSource>();
        audioComplete = GameObject.Find("Audio/Complete").GetComponent<AudioSource>();

        //  load task
        taskSelection.options.Clear();
        string[] taskNameList = Directory.GetFiles("Task/", "*.conf");
        foreach (string taskName in taskNameList) {
            taskSelection.options.Add(new Dropdown.OptionData(taskName.Substring(5, taskName.Length - 10)));
        }
        taskSelection.captionText.text = taskSelection.options[0].text;
        OnValueChange_TaskSelection();
    }
    
	void Update() {
        //  find current selected object
        playProps = GameObject.FindGameObjectsWithTag(TAG_PLAY_PROP);
        selectedObject = technique.Select();

        //  accumulate movement, show sign
        if (experiment != null) {
            experiment.ControllerMove(controller.transform.position);
            signExperimentStart.SetActive(experiment.startSignal);
            signExperimentCompleted.SetActive(experiment.completed);
        }

        //  user event
        if (ViveInput.GetPressDown(handRole, ControllerButton.FullTrigger)) {
            if (experiment != null) {
                int status = experiment.Select(selectedObject);
                if (status == 1) {
                    audioSelectCorrect.Play();
                } else if (status == 0) {
                    audioSelectWrong.Play();
                }
            }
        }
        if (ViveInput.GetPressDown(handRole, ControllerButton.Menu)) {
            OnClick_Start();
        }

        //  color
        foreach (GameObject g in playProps) {
            if (g == selectedObject && g == experiment.targetObject) {
                g.GetComponent<Renderer>().material = Resources.Load("Dark Indigo") as Material;/*
            } else if (g == selectedObject) {
                g.GetComponent<Renderer>().material = Resources.Load("Dark Green") as Material;*/
            } else if (g == experiment.targetObject) {
                g.GetComponent<Renderer>().material = Resources.Load("Dark Blue") as Material;
            } else {
                g.GetComponent<Renderer>().material.color = Color.white;
            }
        }
        
        //  feedback
        ray.SetActive(visibilityRay);
    }

    public string SettingString() {
        return userName.text + "-" + experiment.task + "-" + technique.method + "-" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
    }

    public void OnClick_Start() {
        experiment.Start();
        audioStart.Play();
    }
    public void OnValueChange_TaskSelection() {
        int value = taskSelection.value;
        string task = taskSelection.options[value].text;
        experiment = new Experiment(this, task);
    }
}

class Experiment {
    //  constant
    const int DEFAULT_TRIAL_PER_OBJECT = 3;

    //  main
    public GameObject targetObject = null;
    public bool completed = false;
    public bool startSignal = false;
    public string task;
    Play play;
    bool started = false;
    List<string> record = new List<string>();

    //  next
    string nextPattern = "random";
    List<GameObject> selectableObjects = new List<GameObject>();
    List<GameObject> uniformObjects = new List<GameObject>();

    //  trial
    int trial = 0;
    int trialMax = -1;

    //  measures
    Vector3 prevPosition = Vector3.zero;
    float movementTotal = 0;

    public Experiment(Play play, string task) {
        this.task = task;
        this.play = play;
        Load("Task/" + task + ".conf");
    }

    public void Start() {
        //  init
        started = true;
        completed = false;
        record.Clear();
        record.Add("start " + Technique.TimeString());
        trial = 0;
        movementTotal = 0;
        Next();
        //  start signal
        startSignal = true;
        Timer timer = new Timer(1500);
        timer.Elapsed += new ElapsedEventHandler(StartSignalTimeOut);
        timer.AutoReset = false;
        timer.Enabled = true;
    }

    public int Select(GameObject selectedObject) {
        if (completed) return -1;
        bool correct = (selectedObject == targetObject);
        if (correct) {
            if (started) trial += 1;
            Next();
        }
        if (started) {
            record.Add("select " + targetObject.name + " " + correct + " " + movementTotal + " " + Technique.TimeString());
            if (trial >= trialMax) Complete();
        }
        return correct ? 1 : 0;
    }

    public void ControllerMove(Vector3 nowPosition) {
        if (started) {
            movementTotal += (nowPosition - prevPosition).magnitude;
        }
        prevPosition = nowPosition;
    }

    void Load(string fileName) {
        //  clean up
        completed = false;
        GameObject[] destroyObjects = GameObject.FindGameObjectsWithTag(Play.TAG_PLAY_PROP);
        foreach (GameObject g in destroyObjects) UnityEngine.Object.Destroy(g);

        //  read
        int lineNo = -1;
        GameObject newGameObject = null;
        int selectable = 1;
        StreamReader reader = new StreamReader(new FileStream(fileName, FileMode.Open));
        while (true) {
            lineNo++;
            string line = reader.ReadLine();
            if (line == null) break;
            string[] arr = line.Split(' ');
            switch (arr[0]) {
                case "next":
                    nextPattern = arr[1];
                    break;
                case "trialmax":
                    trialMax = int.Parse(arr[1]);
                    break;
                case "object":
                    PrimitiveType primitiveType = 0;
                    if (arr[1] == "sphere") primitiveType = PrimitiveType.Sphere;
                    newGameObject = GameObject.CreatePrimitive(primitiveType);
                    newGameObject.transform.parent = GameObject.Find("Play").transform;
                    newGameObject.tag = Play.TAG_PLAY_PROP;
                    selectable = 1;
                    break;
                case "name":
                    newGameObject.name = line.Substring(5);
                    break;
                case "selectable":
                    selectable = int.Parse(arr[1]);
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
                    if (selectable == 1) selectableObjects.Add(newGameObject);
                    if (targetObject == null) targetObject = newGameObject;
                    newGameObject = null;
                    break;
            }
        }
        reader.Close();

        //  reload play props
        play.playProps = GameObject.FindGameObjectsWithTag(Play.TAG_PLAY_PROP);

        //  generate uniform gameobjects
        int n = selectableObjects.Count;
        if (trialMax == -1) trialMax = n * DEFAULT_TRIAL_PER_OBJECT;
        if (nextPattern == "uniform") {
            int gt = (trialMax - 1) / n + 1;
            for (int i = 0; i <= gt; i++) {
                int[] ra;
                while (true) {
                    ra = Technique.Shuffle(n);
                    int ulen = uniformObjects.Count;
                    if (ulen == 0 || uniformObjects[ulen - 1] != selectableObjects[ra[0]]) break;
                }
                foreach (int j in ra) uniformObjects.Add(selectableObjects[j]);
            }
            string ss = "";
            foreach (GameObject g in uniformObjects) ss += g.name[8];
            Debug.Log(ss);
        }
    }

    void Next() {
        GameObject prevTargetObject = targetObject;
        if (!started || nextPattern == "random") {
            while (true) {
                int x = Technique.random.Next() % selectableObjects.Count;
                targetObject = play.playProps[x];
                if (targetObject != prevTargetObject) break;
            }
        }
        else if (nextPattern == "uniform") {
            targetObject = uniformObjects[trial];
        }
    }

    void Complete() {
        completed = true;
        StreamWriter writer = new StreamWriter(new FileStream("Log/" + play.SettingString() + ".txt", FileMode.OpenOrCreate));
        foreach (string r in record) writer.WriteLine(r);
        writer.Close();
        play.audioComplete.Play();
    }

    void StartSignalTimeOut(object source, ElapsedEventArgs args) {
        startSignal = false;
    }
}

public class Technique {
    //  constant
    public const float EPS = 1e-5f;

    //  common
    public string method = "~";
    protected Play play;

    protected Technique(Play play) {
        this.play = play;
    }

    public static System.Random random = new System.Random((int)DateTime.Now.Ticks);

    public virtual GameObject Select() { return new GameObject(); }
    
    public static Vector3 QuaternionToVector(Quaternion q) {
        Vector3 r = q.eulerAngles / 180.0f * Mathf.Acos(-1);
        float dx = Mathf.Cos(r.x) * Mathf.Sin(r.y);
        float dy = -Mathf.Sin(r.x);
        float dz = Mathf.Cos(r.x) * Mathf.Cos(r.y);
        return new Vector3(dx, dy, dz);
    }

    public static string TimeString() {
        return DateTime.Now.ToString("HH:mm:ss.ffffff");
    }

    public static int[] Shuffle(int n) {
        int[] a = new int[n];
        for (int i = 0; i < n; i++) a[i] = i;
        for (int i = 0; i < n; i++) {
            int x = random.Next() % n;
            int t = a[i];
            a[i] = a[x];
            a[x] = t;
        }
        return a;
    }
}

class NaiveRay : Technique {
    public NaiveRay(Play play) : base(play) {
        play.visibilityRay = true;
    }

    public override GameObject Select() {
        //  source geomatric data
        Vector3 p = play.controller.transform.position;
        Vector3 v = QuaternionToVector(play.controller.transform.rotation);
        Vector3 e = play.cameraHead.transform.position;
        //  find selected object
        GameObject selectedObject = null;
        float minD = 1e20f;
        foreach (GameObject g in play.playProps) {
            Vector3 q = g.transform.position;
            if (Mathf.Abs(v.x + v.y + v.z) < EPS) continue;
            float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (v.x * v.x + v.y * v.y + v.z * v.z);
            Vector3 i = p + v * t;
            //  if intersect
            if ((q - i).magnitude < g.transform.localScale.x / 2) {
                //  find the first intersected object
                float d = (q - p).magnitude;
                if (d < minD) {
                    minD = d;
                    selectedObject = g;
                }
            }
        }
        return selectedObject;
    }
}

class BubbleRay : NaiveRay {
    //  menu configuration
    public static bool visibilityBubble = false;
    public static bool visibilityFishPole = false;

    //  constant
    Vector3 POINT_HIDE = new Vector3(0, 0, -5);
    Quaternion QUETERNION_NULL = new Quaternion(1, 0, 0, 0);
    Vector3 UNIT_BALL = new Vector3(1, 1, 1);
    Vector3 UNIT_CIRCLE = new Vector3(1, 1, 0.01f);

    //  game object
    GameObject bubble;
    LineRenderer fishPole;

    public BubbleRay(Play play) : base(play) {
        bubble = GameObject.Find("Bubble Ray/Bubble");
        fishPole = GameObject.Find("Bubble Ray/Fish Pole").GetComponent<LineRenderer>();
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
        float range = 0.0f;

        //  intersect
        selectedObject = base.Select();

        //  find minimum distance
        float minF = 1e20f;
        if (selectedObject == null) switch (method) {
            case "BackPlane":
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
                        range = f;
                        minF = f;
                        selectedObject = g;
                    }
                }
                break;

            case "BackSphere":
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
                        range = d;
                        minF = f;
                        selectedObject = g;
                    }
                }
                renderRotation = play.controller.transform.rotation;
                break;
            
            case "HandDistance":
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    if (Mathf.Abs(v.x + v.y + v.z) < EPS) continue;
                    float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (v.x * v.x + v.y * v.y + v.z * v.z);
                    Vector3 i = p + v * t;
                    float f = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (f < minF) {
                        renderPoint = i;
                        range = f;
                        minF = f;
                        selectedObject = g;
                    }
                }
                renderUnit = UNIT_BALL;
                renderRotation = play.controller.transform.rotation;
                break;
                
            case "HandAngular":
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    Vector3 u = q - p;
                    Vector3 w = v.normalized * u.magnitude;
                    Vector3 s = p + w;
                    float d = (q - s).magnitude - g.transform.localScale.x / 2;
                    float f = d / u.magnitude;
                    if (f < minF) {
                        renderPoint = s;
                        range = d;
                        minF = f;
                        selectedObject = g;
                    }
                }
                renderRotation = play.controller.transform.rotation;
                break;

            /*case "EyesDistance":
                foreach (GameObject g in play.playProps) {
                    Vector3 q = g.transform.position;
                    Vector3 n = QuaternionToVector(play.cameraHead.transform.rotation);
                    if (Mathf.Abs(v.x * n.x + v.y * n.y + v.z * n.z) < EPS) continue;
                    float t = -((p.x - q.x) * n.x + (p.y - q.y) * n.y + (p.z - q.z) * n.z) / (v.x * n.x + v.y * n.y + v.z * n.z);
                    Vector3 i = p + v * t;
                    float f = (q - i).magnitude - g.transform.localScale.x / 2;
                    if (f < minF) {
                        renderPoint = i;
                        range = f;
                        minF = f;
                        selectedObject = g;
                    }
                }
                renderUnit = UNIT_BALL;
                renderRotation = play.cameraHead.transform.rotation;
                break;*/
            
            case "EyesAngular":
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
                        range = d;
                        minF = f;
                        selectedObject = g;
                    }
                }
                renderRotation = play.cameraHead.transform.rotation;
                break;
                
            /*case "depth plane":
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
                break; */
        }

        //  draw bubble
        float renderScale = range * 2;
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
