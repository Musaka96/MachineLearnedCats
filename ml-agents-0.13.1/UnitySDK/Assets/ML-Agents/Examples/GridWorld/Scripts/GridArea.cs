using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MLAgents;


public class GridArea : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> actorObjs;
    [HideInInspector]
    public int[] players;

    public GameObject trueAgent;

    IFloatProperties m_ResetParameters;

    Camera m_AgentCam;

    public GameObject goalPref;
    public GameObject pitPref;
    GameObject[] m_Objects;

    GameObject m_Plane;
    GameObject m_Sn;
    GameObject m_Ss;
    GameObject m_Se;
    GameObject m_Sw;

    Vector3 m_InitialPosition;

    public void Start()
    {
        this.m_ResetParameters = FindObjectOfType<Academy>().FloatProperties;

        this.m_Objects = new[] { this.goalPref, this.pitPref };

        this.m_AgentCam = this.transform.Find("agentCam").GetComponent<Camera>();

        this.actorObjs = new List<GameObject>();

        var sceneTransform = this.transform.Find("scene");

        this.m_Plane = sceneTransform.Find("Plane").gameObject;
        this.m_Sn = sceneTransform.Find("sN").gameObject;
        this.m_Ss = sceneTransform.Find("sS").gameObject;
        this.m_Sw = sceneTransform.Find("sW").gameObject;
        this.m_Se = sceneTransform.Find("sE").gameObject;
        this.m_InitialPosition = this.transform.position;
    }

    public void SetEnvironment()
    {
        this.transform.position = this.m_InitialPosition * (this.m_ResetParameters.GetPropertyWithDefault("gridSize", 5f) + 1);
        var playersList = new List<int>();

        for (var i = 0; i < (int)this.m_ResetParameters.GetPropertyWithDefault("numObstacles", 1); i++)
        {
            playersList.Add(1);
        }

        for (var i = 0; i < (int)this.m_ResetParameters.GetPropertyWithDefault("numGoals", 1f); i++)
        {
            playersList.Add(0);
        }
        this.players = playersList.ToArray();

        var gridSize = (int)this.m_ResetParameters.GetPropertyWithDefault("gridSize", 5f);
        this.m_Plane.transform.localScale = new Vector3(gridSize / 10.0f, 1f, gridSize / 10.0f);
        this.m_Plane.transform.localPosition = new Vector3((gridSize - 1) / 2f, -0.5f, (gridSize - 1) / 2f);
        this.m_Sn.transform.localScale = new Vector3(1, 1, gridSize + 2);
        this.m_Ss.transform.localScale = new Vector3(1, 1, gridSize + 2);
        this.m_Sn.transform.localPosition = new Vector3((gridSize - 1) / 2f, 0.0f, gridSize);
        this.m_Ss.transform.localPosition = new Vector3((gridSize - 1) / 2f, 0.0f, -1);
        this.m_Se.transform.localScale = new Vector3(1, 1, gridSize + 2);
        this.m_Sw.transform.localScale = new Vector3(1, 1, gridSize + 2);
        this.m_Se.transform.localPosition = new Vector3(gridSize, 0.0f, (gridSize - 1) / 2f);
        this.m_Sw.transform.localPosition = new Vector3(-1, 0.0f, (gridSize - 1) / 2f);

        this.m_AgentCam.orthographicSize = (gridSize) / 2f;
        this.m_AgentCam.transform.localPosition = new Vector3((gridSize - 1) / 2f, gridSize + 1f, (gridSize - 1) / 2f);
    }

    public void AreaReset()
    {
        var gridSize = (int)this.m_ResetParameters.GetPropertyWithDefault("gridSize", 5f); ;
        foreach (var actor in this.actorObjs)
        {
            DestroyImmediate(actor);
        }
        this.SetEnvironment();

        this.actorObjs.Clear();

        var numbers = new HashSet<int>();
        while (numbers.Count < this.players.Length + 1)
        {
            numbers.Add(Random.Range(0, gridSize * gridSize));
        }
        var numbersA = Enumerable.ToArray(numbers);

        for (var i = 0; i < this.players.Length; i++)
        {
            var x = (numbersA[i]) / gridSize;
            var y = (numbersA[i]) % gridSize;
            var actorObj = Instantiate(this.m_Objects[this.players[i]], this.transform);
            actorObj.transform.localPosition = new Vector3(x, -0.25f, y);
            this.actorObjs.Add(actorObj);
        }

        var xA = (numbersA[this.players.Length]) / gridSize;
        var yA = (numbersA[this.players.Length]) % gridSize;
        this.trueAgent.transform.localPosition = new Vector3(xA, -0.25f, yA);
    }
}
