using System;
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
    public Technique technique = null;
    
    void Start() {
        //  gameobject
        cameraHead = GameObject.Find("VROrigin/[CameraRig]/Camera (eye)");
        controllerLeft = GameObject.Find("VROrigin/[CameraRig]/Controller (left)");
        controllerRight = GameObject.Find("VROrigin/[CameraRig]/Controller (right)");
        controller = controllerRight;
        handRole = HandRole.RightHand;

        signExperimentCompleted = GameObject.Find("Sign Experiment Completed");
        signExperimentStart = GameObject.Find("Sign Experiment Start");
        taskSelection = GameObject.Find("Task Selection").GetComponent<Dropdown>();
        userName = GameObject.Find("User Name Text").GetComponent<Text>();

        audioSelectCorrect = GameObject.Find("Audio/Select Correct").GetComponent<AudioSource>();
        audioSelectWrong = GameObject.Find("Audio/Select Wrong").GetComponent<AudioSource>();
        audioStart = GameObject.Find("Audio/Start").GetComponent<AudioSource>();
        audioComplete = GameObject.Find("Audio/Complete").GetComponent<AudioSource>();

        //  init static
        NaiveRay.ray = GameObject.Find("Cue/Ray");
        NaiveRay.ray.transform.SetParent(controller.transform);
        NaiveRay.ray.transform.localPosition = new Vector3(0, 0, 500);
        NaiveRay.ray.transform.localRotation = Quaternion.Euler(90, 0, 0);
        NaiveRay.ray.transform.localScale = new Vector3(0.01f, 500, 0.01f);
        NaiveRay.ray.SetActive(false);

        BubbleRay.bubble = GameObject.Find("Cue/Bubble");
        BubbleRay.fishPole = GameObject.Find("Cue/Fish Pole");

        HeuristicRay.fishPole = GameObject.Find("Cue/Fish Pole");
        HeuristicRay.fishPole.SetActive(false);

        X3DBubbleCursor.cursor = GameObject.Find("Cue/Cursor");
        X3DBubbleCursor.cursor.transform.SetParent(controller.transform);
        X3DBubbleCursor.cursor.transform.localPosition = new Vector3(0, 0, 0.1f);
        X3DBubbleCursor.cursor.transform.localRotation = Quaternion.Euler(0, 0, 0);
        X3DBubbleCursor.cursor.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        X3DBubbleCursor.cursor.SetActive(false);
        X3DBubbleCursor.bubble = GameObject.Find("Cue/Bubble");
        X3DBubbleCursor.bubble.SetActive(false);
        X3DBubbleCursor.bubble2 = GameObject.Find("Cue/Bubble 2");
        X3DBubbleCursor.bubble2.SetActive(false);

        GoGo.hand = GameObject.Find("Cue/hand_right_prefab");
        GoGo.hand.transform.SetParent(controller.transform);
        GoGo.hand.transform.localPosition = new Vector3(0, 0, 0);
        GoGo.hand.transform.localRotation = Quaternion.Euler(0, 0, 0);
        GoGo.hand.transform.localScale = new Vector3(1, 1, 1);
        GoGo.hand.SetActive(false);

        //  load task
        taskSelection.options.Clear();
        string[] taskNameList = Directory.GetFiles("Task/", "*.conf");
        foreach (string taskName in taskNameList) {
            taskSelection.options.Add(new Dropdown.OptionData(taskName.Substring(5, taskName.Length - 10)));
        }
        taskSelection.captionText.text = taskSelection.options[0].text;
        OnValueChange_TaskSelection();

        //  main
        ChangeTechnique<Technique>();
    }
    
	void Update() {
        //  find current selected object
        playProps = GameObject.FindGameObjectsWithTag(TAG_PLAY_PROP);
        technique.Update();
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
    }

    public void ChangeTechnique<T>() where T : Technique, new() {
        if (technique != null) technique.Deconstruct();
        technique = new T();
    }

    public string SettingString() {
        return userName.text + "-" + experiment.task + "-" + technique.method.Replace(" ", "") + "-" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
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
            /*string ss = "";
            foreach (GameObject g in uniformObjects) ss += g.name[8];
            Debug.Log(ss);*/
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
    public const float EPS = 1e-5f;
    public static System.Random random = new System.Random((int)DateTime.Now.Ticks);
    public string method = "~";
    public Play play;
    
    public Technique() {
        play = GameObject.Find("Play").GetComponent<Play>();
    }
    public virtual void Deconstruct() {
    }
    public virtual GameObject Select() {
        return null;
    }
    public virtual void Update() {

    }

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
    public static GameObject ray;

    public NaiveRay() : base() {
        method = "NaiveRay";
        ray.SetActive(true);
    }

    public override void Deconstruct() {
        ray.SetActive(false);
        base.Deconstruct();
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
    public static GameObject bubble;
    public static GameObject fishPole;

    //  menu configuration
    public static bool visibilityBubble = true;
    public static bool visibilityFishPole = true;

    //  constant
    Vector3 POINT_HIDE = new Vector3(0, 0, -5);
    Quaternion QUETERNION_NULL = new Quaternion(1, 0, 0, 0);
    Vector3 UNIT_BALL = new Vector3(1, 1, 1);
    Vector3 UNIT_CIRCLE = new Vector3(1, 1, 0.01f);

    public BubbleRay() : base() {
        method = "Hand Distance";
        fishPole.SetActive(true);
        bubble.SetActive(true);
    }

    public override void Deconstruct() {
        fishPole.SetActive(false);
        bubble.SetActive(false);
        base.Deconstruct();
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
        if (selectedObject == null)
            switch (method) {
                case "Back Plane":
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

                case "Back Sphere":
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

                case "Hand Distance":
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

                case "Hand Angular":
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

                    /*case "Dynamic Depth Plane":
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

                    case "Dynamic Depth Sphere":
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
                        break;*/
            }
        DrawFishPole(p, selectedObject.transform.position, v);
        DrawBubble(renderPoint, renderRotation, renderUnit, range);
        return selectedObject;
    }

    void DrawBubble(Vector3 renderPoint, Quaternion renderRotation, Vector3 renderUnit, float range) {
        float renderScale = range * 2;
        Transform transformBubble = bubble.transform;
        transformBubble.position = (visibilityBubble) ? renderPoint : POINT_HIDE;
        transformBubble.rotation = renderRotation;
        transformBubble.localScale = renderUnit * ((visibilityBubble) ? renderScale : 1);
    }

    void DrawFishPole(Vector3 p, Vector3 q, Vector3 v) {
        LineRenderer fishPoleRenderer = fishPole.GetComponent<LineRenderer>();
        if (visibilityFishPole) {
            float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (v.x * v.x + v.y * v.y + v.z * v.z);
            Vector3 r = p + v * t * 0.8f;
            List<Vector3> bs = new List<Vector3>();
            for (int i = 0; i <= 100; i++) {
                float j = i / 100.0f;
                Vector3 b = (1 - j) * (1 - j) * p + 2 * j * (1 - j) * r + j * j * q;
                bs.Add(b);
            }
            fishPoleRenderer.SetPositions(bs.ToArray());
            fishPoleRenderer.positionCount = bs.Count;
        }
        else {
            fishPoleRenderer.positionCount = 0;
        }
    }
}

class HeuristicRay : NaiveRay {
    public static GameObject fishPole;
    public HeuristicRay() : base() {
        method = "Heuristic Ray";
        fishPole.SetActive(true);
    }

    public override void Deconstruct() {
        fishPole.SetActive(false);
        base.Deconstruct();
    }
}

class X3DBubbleCursor : Technique {
    const float BUBBLE_SPLIT_GAP = 0.02f;
    const float BUBBLE2_SCALE_RATIO = 1.2f;
    public static GameObject cursor;
    public static GameObject bubble;
    public static GameObject bubble2;

    public X3DBubbleCursor() : base() {
        method = "3D Bubble Cursor";
        cursor.SetActive(true);
        bubble.SetActive(true);
        bubble2.SetActive(true);
    }

    public override void Deconstruct() {
        cursor.SetActive(false);
        bubble.SetActive(false);
        bubble2.SetActive(false);
        base.Deconstruct();
    }

    public override GameObject Select() {
        GameObject selectedObject = null;
        float minD = 1e20f, secD = 1e20f;
        foreach (GameObject g in play.playProps) {
            float d = (cursor.transform.position - g.transform.position).magnitude - g.transform.localScale.x / 2;
            if (d < minD) {
                secD = minD;
                minD = d;
                selectedObject = g;
            } else {
                secD = Math.Min(secD, d);
            }
        }
        float renderScale = Math.Min(minD + selectedObject.transform.localScale.x, secD - BUBBLE_SPLIT_GAP) * 2;
        bubble.transform.position = cursor.transform.position;
        bubble.transform.localScale = new Vector3(1, 1, 1) * renderScale;
        bubble2.transform.position = selectedObject.transform.position;
        bubble2.transform.localScale = selectedObject.transform.localScale * BUBBLE2_SCALE_RATIO;
        return selectedObject;
    }
}

class GoGo : Technique {
    const float LINEAR_RANGE = 0.3f;
    const float NONLINEAR_RATIO = 100.0f;
    const float TOUCH_RANGE = 0.2f;

    public static GameObject hand;

    public GoGo() : base() {
        hand.SetActive(true);
    }

    public override void Deconstruct() {
        hand.SetActive(false);
        base.Deconstruct();
    }

    public override GameObject Select() {
        GameObject selectedObject = null;
        float minD = TOUCH_RANGE;
        foreach (GameObject g in play.playProps) {
            float d = (g.transform.position - hand.transform.position).magnitude - g.transform.localScale.x / 2;
            if (d < minD) {
                minD = d;
                selectedObject = g;
            }
        }
        return selectedObject;
    }

    public override void Update() {
        Vector3 o = play.cameraHead.transform.position - new Vector3(0, 0.35f, 0);
        Vector3 v = play.controller.transform.position - o;
        if (v.magnitude <= LINEAR_RANGE) {
            hand.transform.position = o + v;
        }
        else {
            hand.transform.position = o + v.normalized * (v.magnitude + NONLINEAR_RATIO * Mathf.Pow(v.magnitude - LINEAR_RANGE, 2));
        }
        hand.transform.rotation = play.controller.transform.rotation;
    }
}

