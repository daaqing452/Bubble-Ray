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
    public static Material MATERIAL_DEFAULT;

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
    Experiment experiment = null;
    public Technique technique = null;
    
    void Start() {
        //  gameobject
        MATERIAL_DEFAULT = Resources.Load("Trans White") as Material;

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
        BubbleRay.fishPole.SetActive(false);
        
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

        NaiveCone.cone = GameObject.Find("Cue/Cone");
        NaiveCone.cone.transform.SetParent(controller.transform);
        NaiveCone.cone.transform.position = new Vector3(0, 0, 0);
        NaiveCone.cone.SetActive(false);

        SQUADCone.squad = GameObject.Find("Cue/SQUAD");
        SQUADCone.cursor = GameObject.Find("Cue/SQUAD/Cursor");
        SQUADCone.squad.SetActive(false);

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
        playProps = GameObject.FindGameObjectsWithTag(TAG_PLAY_PROP);
        technique.Update();

        //  user event
        bool experimentCheck = false;
        if (ViveInput.GetPressDown(handRole, ControllerButton.FullTrigger)) experimentCheck = technique.Trigger();
        if (ViveInput.GetPressDown(handRole, ControllerButton.Menu)) OnClick_Start();
        
        //  find current selected object
        List<GameObject> selectedObjects = technique.Select();

        //  experiment check
        if (experimentCheck && experiment != null) {
            GameObject selectedObject = (selectedObjects.Count == 0) ? null : selectedObjects[0];
            int status = experiment.Select(selectedObject);
            if (status == 1) audioSelectCorrect.Play(); else if (status == 0) audioSelectWrong.Play();
        }

        //  accumulate movement, show sign
        if (experiment != null) {
            experiment.ControllerMove(controller.transform.position);
            signExperimentStart.SetActive(experiment.startSignal);
            signExperimentCompleted.SetActive(experiment.completed);
        }
        
        //  color
        foreach (GameObject g in playProps) {
            if (selectedObjects.Contains(g) && g == experiment.targetObject) {
                ChangeColor(g, Color.cyan);
            } else if (g == experiment.targetObject) {
                ChangeColor(g, Color.blue);
            } else if (selectedObjects.Contains(g) && technique.selectAndColor) {
                ChangeColor(g, Color.green);
            } else {
                ChangeColor(g, Color.white);
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
    
    public static void ChangeColor(GameObject g, Color c, float a = -1) {
        if (a == -1) a = g.GetComponent<Renderer>().material.color.a;
        Color c1 = new Color(c.r, c.g, c.b, a);
        g.GetComponent<Renderer>().material.color = c1;
    }
    public static void ChangeColor(GameObject g, GameObject h, float a = -1) {
        ChangeColor(g, h.GetComponent<Renderer>().material.color, a);
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
        GameObject newObject = null;
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
                    newObject = GameObject.CreatePrimitive(primitiveType);
                    newObject.transform.SetParent(GameObject.Find("Play").transform);
                    newObject.tag = Play.TAG_PLAY_PROP;
                    newObject.GetComponent<Renderer>().material = Play.MATERIAL_DEFAULT;
                    selectable = 1;
                    break;
                case "name":
                    newObject.name = line.Substring(5);
                    break;
                case "selectable":
                    selectable = int.Parse(arr[1]);
                    break;
                case "position":
                    float px = float.Parse(arr[1]), py = float.Parse(arr[2]), pz = float.Parse(arr[3]);
                    newObject.transform.position = new Vector3(px, py, pz);
                    break;
                case "rotation":
                    float rx = float.Parse(arr[1]), ry = float.Parse(arr[2]), rz = float.Parse(arr[3]);
                    newObject.transform.rotation = new Quaternion(rx, ry, rz, 1);
                    break;
                case "scale":
                    float sx = float.Parse(arr[1]), sy = float.Parse(arr[2]), sz = float.Parse(arr[3]);
                    newObject.transform.localScale = new Vector3(sx, sy, sz);
                    break;
                case "color":
                    float cr = float.Parse(arr[1]), cg = float.Parse(arr[2]), cb = float.Parse(arr[3]);
                    newObject.GetComponent<Renderer>().material.color = new Color(cr, cg, cb);
                    break;
                case "end":
                    if (selectable == 1) selectableObjects.Add(newObject);
                    if (targetObject == null) targetObject = newObject;
                    newObject = null;
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
    public static float PI = Mathf.Acos(-1);
    public const float EPS = 1e-5f;
    public Vector3 POINT_HIDE = new Vector3(0, 0, -5);
    public Quaternion QUETERNION_NULL = new Quaternion(1, 0, 0, 0);
    public Vector3 UNIT_BALL = new Vector3(1, 1, 1);
    public Vector3 UNIT_CIRCLE = new Vector3(1, 1, 0.01f);

    //  main
    public static System.Random random = new System.Random((int)DateTime.Now.Ticks);
    public string method = "~";
    public bool selectAndColor = false;
    public Play play;
    
    public Technique() {
        play = GameObject.Find("Play").GetComponent<Play>();
    }
    public virtual void Deconstruct() {
    }
    public virtual List<GameObject> Select() {
        return new List<GameObject>();
    }
    public virtual void Update() {

    }
    public virtual bool Trigger() {
        return true;
    }

    public static int Sqr(int x) {
        return x * x;
    }
    public static float Sqr(float x) {
        return x * x;
    }
    public static Vector3 Quaternion2Vector(Quaternion q) {
        Vector3 r = q.eulerAngles / 180.0f * PI;
        Vector3 v;
        v.x = Mathf.Cos(r.x) * Mathf.Sin(r.y);
        v.y = -Mathf.Sin(r.x);
        v.z = Mathf.Cos(r.x) * Mathf.Cos(r.y);
        return v;
    }
    public static Quaternion Vector2Quaternion(Vector3 v) {
        v = v.normalized;
        Vector3 r;
        r.x = -Mathf.Asin(v.y);
        r.y = Mathf.Asin(v.x / Mathf.Cos(r.x));
        r.z = 0;
        r = r / PI * 180.0f;
        return Quaternion.Euler(r.x, r.y, r.z);
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
    protected void DrawFishPole(GameObject fishPole, bool visible, Vector3 p, Vector3 q, Vector3 v) {
        LineRenderer fishPoleRenderer = fishPole.GetComponent<LineRenderer>();
        if (visible) {
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
    protected void DrawBubble(GameObject bubble, bool visible, Vector3 renderPoint, Quaternion renderRotation, Vector3 renderUnit, float renderScale) {
        renderScale *= 2;
        Transform transformBubble = bubble.transform;
        transformBubble.position = (visible) ? renderPoint : POINT_HIDE;
        transformBubble.rotation = renderRotation;
        transformBubble.localScale = renderUnit * ((visible) ? renderScale : 1);
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

    public override List<GameObject> Select() {
        //  source geomatric data
        Vector3 p = play.controller.transform.position;
        Vector3 v = Quaternion2Vector(play.controller.transform.rotation);
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
        List<GameObject> selectedObjects = new List<GameObject>();
        if (selectedObject != null) selectedObjects.Add(selectedObject);
        return selectedObjects;
    }
}

class BubbleRay : NaiveRay {
    public static GameObject bubble;
    public static GameObject fishPole;

    //  menu configuration
    public static bool visibilityBubble = true;
    public static bool visibilityFishPole = true;
    
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

    public override List<GameObject> Select() {
        //  source geomatric data
        Vector3 p = play.controller.transform.position;
        Vector3 v = Quaternion2Vector(play.controller.transform.rotation);
        Vector3 e = play.cameraHead.transform.position;

        //  initiate
        GameObject selectedObject = null;
        Vector3 renderPoint = POINT_HIDE;
        Quaternion renderRotation = QUETERNION_NULL;
        Vector3 renderUnit = UNIT_CIRCLE;
        float renderScale = 0.0f;

        //  intersect
        List<GameObject> selectedObjects = base.Select();

        //  find minimum distance
        float minF = 1e20f;
        if (selectedObjects.Count > 0) {
            selectedObject = selectedObjects[0];
        } else {
            switch (method) {
                // dot(p+vt-q, v)=0
                case "Hand Distance":
                    foreach (GameObject g in play.playProps) {
                        Vector3 q = g.transform.position;
                        if (Mathf.Abs(v.x + v.y + v.z) < EPS) continue;
                        float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (Sqr(v.x) + Sqr(v.y) + Sqr(v.z));
                        Vector3 i = p + v * t;
                        float f = (q - i).magnitude - g.transform.localScale.x / 2;
                        if (f < minF) {
                            renderPoint = i;
                            renderScale = f;
                            minF = f;
                            selectedObject = g;
                        }
                    }
                    renderUnit = UNIT_BALL;
                    break;

                case "Hand Angular":
                    float maxDepth = 100;
                    foreach (GameObject g in play.playProps) maxDepth = Mathf.Max(maxDepth, (g.transform.position - p).magnitude + g.transform.localScale.x);
                    foreach (GameObject g in play.playProps) {
                        Vector3 q = g.transform.position;
                        if (Mathf.Abs(v.x + v.y + v.z) < EPS) continue;
                        float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (Sqr(v.x) + Sqr(v.y) + Sqr(v.z));
                        Vector3 i = p + v * t;
                        float r = g.transform.localScale.x / 2;
                        float f = Mathf.Atan(((i - q).magnitude - r) / (i - p).magnitude);
                        if (f < minF) {
                            Vector3 j = e + (q - e).normalized * maxDepth;
                            Vector3 k = p + v.normalized * maxDepth;
                            renderPoint = k;
                            renderScale = (j - k).magnitude - r / (q - e).magnitude * maxDepth;
                            minF = f;
                            selectedObject = g;
                        }
                    }
                    renderRotation = Vector2Quaternion(renderPoint - e);
                    break;
                /*case "Back Plane":
                case "Back Sphere":
                case "Dynamic Depth Plane":
                case "Dynamic Depth Sphere":*/
            }
            selectedObjects.Add(selectedObject);
        }
        DrawFishPole(fishPole, visibilityFishPole, p, selectedObject.transform.position, v);
        DrawBubble(bubble, visibilityBubble, renderPoint, renderRotation, renderUnit, renderScale);
        return selectedObjects;
    }
}

class HeuristicRay : NaiveRay {
    const float ANGLE_LIMIT = 0.2f;
    const float ACCUMULATE_RATE = 0.05f;
    int n = 0;
    float[] score;

    public HeuristicRay() : base() {
        method = "Heuristic Ray";
        selectAndColor = true;
    }

    public override List<GameObject> Select() {
        if (n != play.playProps.Length) {
            n = play.playProps.Length;
            score = new float[n];
        }
        Vector3 p = play.controller.transform.position;
        Vector3 v = Quaternion2Vector(play.controller.transform.rotation);
        GameObject selectedObject = null;
        float maxS = -1;
        for (int i = 0; i < n; i++) {
            GameObject g = play.playProps[i];
            Vector3 q = g.transform.position;
            Vector3 u = q - p;
            Vector3 w = v.normalized * u.magnitude;
            Vector3 s = p + w;
            float alpha = Mathf.Atan((q - s).magnitude / u.magnitude);
            float nowScore = Mathf.Max(1 - alpha / ANGLE_LIMIT, 0);
            score[i] = score[i] * (1 - ACCUMULATE_RATE) + nowScore * ACCUMULATE_RATE;
            if (score[i] > maxS) {
                maxS = score[i];
                selectedObject = g;
            }
        }
        List<GameObject> selectedObjects = new List<GameObject>();
        selectedObjects.Add(selectedObject);
        return selectedObjects;
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

    public override List<GameObject> Select() {
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
        float renderScale = Math.Min(minD + selectedObject.transform.localScale.x, secD - BUBBLE_SPLIT_GAP);
        DrawBubble(bubble, true, cursor.transform.position, QUETERNION_NULL, UNIT_BALL, renderScale);
        DrawBubble(bubble2, true, selectedObject.transform.position, QUETERNION_NULL, UNIT_BALL, selectedObject.transform.localScale.x / 2 * BUBBLE2_SCALE_RATIO);
        List<GameObject> selectedObjects = new List<GameObject>();
        selectedObjects.Add(selectedObject);
        return selectedObjects;
    }
}

class GoGo : Technique {
    public static GameObject hand;
    const float LINEAR_RANGE = 0.3f;
    const float NONLINEAR_RATIO = 100.0f;
    const float TOUCH_RANGE = 0.2f;

    public GoGo() : base() {
        hand.SetActive(true);
    }

    public override void Deconstruct() {
        hand.SetActive(false);
        base.Deconstruct();
    }

    public override List<GameObject> Select() {
        GameObject selectedObject = null;
        float minD = TOUCH_RANGE;
        foreach (GameObject g in play.playProps) {
            float d = (g.transform.position - hand.transform.position).magnitude - g.transform.localScale.x / 2;
            if (d < minD) {
                minD = d;
                selectedObject = g;
            }
        }
        List<GameObject> selectedObjects = new List<GameObject>();
        if (selectedObject != null) selectedObjects.Add(selectedObject);
        return selectedObjects;
    }

    public override void Update() {
        Vector3 o = play.cameraHead.transform.position - new Vector3(0, 0.35f, 0);
        float d = (play.controller.transform.position - o).magnitude;
        if (d <= LINEAR_RANGE) {
            hand.transform.localPosition = new Vector3(0, 0, 0);
        } else {
            hand.transform.localPosition = new Vector3(0, 0, NONLINEAR_RATIO * Mathf.Pow(d - LINEAR_RANGE, 2));
        }
    }
}

class NaiveCone : Technique {
    public static GameObject cone;

    public NaiveCone() : base() {
        cone.SetActive(true);
    }

    public override void Deconstruct() {
        cone.SetActive(false);
        base.Deconstruct();
    }
}

class SQUADCone : NaiveCone {
    public static GameObject squad;
    public static GameObject cursor;
    const float ANGLE_CONE = 0.13f;
    const string OBJECT_ON_SQUAD_TAG = "on squad";
    const float OBJECT_ON_SQUAD_SCALE = 40;
    const float OBJECT_ON_SQUAD_MARGIN = 50;
    const float OBJECT_ON_SQUAD_DEPTH = 500;

    int step = 0;
    bool updateSquad = false;
    List<GameObject> selectedObjects = new List<GameObject>();

    public SQUADCone() {
        selectAndColor = true;
    }

    public override void Deconstruct() {
        squad.SetActive(false);
        base.Deconstruct();
    }

    public override List<GameObject> Select() {
        if (step == 0) {
            selectedObjects.Clear();
            Vector3 p = play.controller.transform.position;
            Vector3 v = Quaternion2Vector(play.controller.transform.rotation);
            foreach (GameObject g in play.playProps) {
                Vector3 q = g.transform.position;
                if (Mathf.Abs(v.x + v.y + v.z) < EPS) continue;
                float t = -((p.x - q.x) * v.x + (p.y - q.y) * v.y + (p.z - q.z) * v.z) / (Sqr(v.x) + Sqr(v.y) + Sqr(v.z));
                Vector3 i = p + v * t;
                float r = g.transform.localScale.x / 2;
                float angle = Mathf.Atan(((i - q).magnitude - r) / (i - p).magnitude);
                if (angle > ANGLE_CONE) continue;
                selectedObjects.Add(g);
            }
            return selectedObjects;
        } else if (step == 1) {
            return new List<GameObject>();
        } else if (step == 2) {
            step = 0;
            return selectedObjects;
        }
        return new List<GameObject>();
    }

    public override void Update() {
        if (step != 1) return;
        if (updateSquad) {
            DrawSquad();
            updateSquad = false;
        }
        Vector3 p = play.controller.transform.position;
        Vector3 v = Quaternion2Vector(play.controller.transform.rotation);
        Vector3 o = play.cameraHead.transform.position;
        Vector3 u = Quaternion2Vector(play.cameraHead.transform.rotation);
        Vector3 q = o + u.normalized * 450;
        float t = ((q.x - p.x) * u.x + (q.y - p.y) * u.y + (q.z - p.z) * u.z) / (v.x * u.x + v.y * u.y + v.z * u.z);
        cursor.transform.position = p + v * t;
    }
    
    public override bool Trigger() {
        if (step == 0) {
            if (selectedObjects.Count == 1) return true;
            squad.SetActive(true);
            foreach (GameObject g in play.playProps) Play.ChangeColor(g, g, 0.1f);
            updateSquad = true;
            step = 1;
            return false;
        } else if (step == 1) {
            int b = BelongGroup(cursor.transform.localPosition);
            List<GameObject> newSelectedObjects = new List<GameObject>();
            for (int i = b; i < selectedObjects.Count; i += 4) newSelectedObjects.Add(selectedObjects[i]);
            if (newSelectedObjects.Count == 0) return false;
            selectedObjects = newSelectedObjects;
            if (selectedObjects.Count == 1) {
                squad.SetActive(false);
                foreach (GameObject g in play.playProps) Play.ChangeColor(g, g, 1.0f);
                step = 2;
                return true;
            } else {
                updateSquad = true;
                return false;
            }
        }
        return false;
    }

    void DrawSquad() {
        GameObject[] objectsOnSquad = GameObject.FindGameObjectsWithTag(OBJECT_ON_SQUAD_TAG);
        foreach (GameObject g in objectsOnSquad) UnityEngine.Object.Destroy(g);
        for (int i = 0; i < selectedObjects.Count; i++) {
            GameObject g = selectedObjects[i];
            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newObject.transform.SetParent(squad.transform);
            newObject.tag = OBJECT_ON_SQUAD_TAG;
            newObject.GetComponent<Renderer>().material = g.GetComponent<Renderer>().material;
            Play.ChangeColor(newObject, g, 1.0f);
            Vector3 p = GeneratePosition(i % 4, i / 4) * OBJECT_ON_SQUAD_MARGIN;
            p.z = OBJECT_ON_SQUAD_DEPTH;
            newObject.transform.localPosition = p;
            newObject.transform.localScale = UNIT_BALL * OBJECT_ON_SQUAD_SCALE;
        }
    }

    Vector3 GeneratePosition(int group, int index) {
        index++;
        int row = (int)(Math.Sqrt(index) - 1e-10) + 1;
        int column = Sqr(row) - index - (row - 1);
        switch (group) {
            case 0: return new Vector3(-column, row, 0);
            case 1: return new Vector3(row, column, 0);
            case 2: return new Vector3(column, -row, 0);
            case 3: return new Vector3(-row, -column, 0);
            default: return new Vector3(0, 0, 0);
        }
    }

    int BelongGroup(Vector3 p) {
        if (p.y >= 0 && Mathf.Abs(p.x) <= Mathf.Abs(p.y)) return 0;
        if (p.x >= 0 && Mathf.Abs(p.y) <= Mathf.Abs(p.x)) return 1;
        if (p.y <= 0 && Mathf.Abs(p.x) <= Mathf.Abs(p.y)) return 2;
        if (p.x <= 0 && Mathf.Abs(p.y) <= Mathf.Abs(p.x)) return 3;
        return -1;
    }
}
