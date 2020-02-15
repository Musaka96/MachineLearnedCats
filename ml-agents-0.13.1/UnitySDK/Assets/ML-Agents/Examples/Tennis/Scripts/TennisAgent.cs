using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class TennisAgent : Agent
{
    [Header("Specific to Tennis")]
    public GameObject ball;
    public bool invertX;
    public int score;
    public GameObject myArea;
    public float angle;
    public float scale;

    Text m_TextComponent;
    Rigidbody m_AgentRb;
    Rigidbody m_BallRb;
    float m_InvertMult;
    IFloatProperties m_ResetParams;

    // Looks for the scoreboard based on the name of the gameObjects.
    // Do not modify the names of the Score GameObjects
    const string k_CanvasName = "Canvas";
    const string k_ScoreBoardAName = "ScoreA";
    const string k_ScoreBoardBName = "ScoreB";

    public override void InitializeAgent()
    {
        this.m_AgentRb = this.GetComponent<Rigidbody>();
        this.m_BallRb = this.ball.GetComponent<Rigidbody>();
        var canvas = GameObject.Find(k_CanvasName);
        GameObject scoreBoard;
        var academy = FindObjectOfType<Academy>();
        this.m_ResetParams = academy.FloatProperties;
        if (this.invertX)
        {
            scoreBoard = canvas.transform.Find(k_ScoreBoardBName).gameObject;
        }
        else
        {
            scoreBoard = canvas.transform.Find(k_ScoreBoardAName).gameObject;
        }
        this.m_TextComponent = scoreBoard.GetComponent<Text>();
        this.SetResetParameters();
    }

    public override void CollectObservations()
    {
        this.AddVectorObs(this.m_InvertMult * (this.transform.position.x - this.myArea.transform.position.x));
        this.AddVectorObs(this.transform.position.y - this.myArea.transform.position.y);
        this.AddVectorObs(this.m_InvertMult * this.m_AgentRb.velocity.x);
        this.AddVectorObs(this.m_AgentRb.velocity.y);

        this.AddVectorObs(this.m_InvertMult * (this.ball.transform.position.x - this.myArea.transform.position.x));
        this.AddVectorObs(this.ball.transform.position.y - this.myArea.transform.position.y);
        this.AddVectorObs(this.m_InvertMult * this.m_BallRb.velocity.x);
        this.AddVectorObs(this.m_BallRb.velocity.y);
    }

    public override void AgentAction(float[] vectorAction)
    {
        var moveX = Mathf.Clamp(vectorAction[0], -1f, 1f) * this.m_InvertMult;
        var moveY = Mathf.Clamp(vectorAction[1], -1f, 1f);

        if (moveY > 0.5 && this.transform.position.y - this.transform.parent.transform.position.y < -1.5f)
        {
            this.m_AgentRb.velocity = new Vector3(this.m_AgentRb.velocity.x, 7f, 0f);
        }

        this.m_AgentRb.velocity = new Vector3(moveX * 30f, this.m_AgentRb.velocity.y, 0f);

        if (this.invertX && this.transform.position.x - this.transform.parent.transform.position.x < -this.m_InvertMult ||
            !this.invertX && this.transform.position.x - this.transform.parent.transform.position.x > -this.m_InvertMult)
        {
            this.transform.position = new Vector3(-this.m_InvertMult + this.transform.parent.transform.position.x,
                this.transform.position.y,
                this.transform.position.z);
        }

        this.m_TextComponent.text = this.score.ToString();
    }

    public override float[] Heuristic()
    {
        var action = new float[2];

        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        return action;
    }

    public override void AgentReset()
    {
        this.m_InvertMult = this.invertX ? -1f : 1f;

        this.transform.position = new Vector3(-this.m_InvertMult * Random.Range(6f, 8f), -1.5f, -3.5f) + this.transform.parent.transform.position;
        this.m_AgentRb.velocity = new Vector3(0f, 0f, 0f);

        this.SetResetParameters();
    }

    public void SetRacket()
    {
        this.angle = this.m_ResetParams.GetPropertyWithDefault("angle", 55);
        this.gameObject.transform.eulerAngles = new Vector3(
            this.gameObject.transform.eulerAngles.x,
            this.gameObject.transform.eulerAngles.y,
            this.m_InvertMult * this.angle
        );
    }

    public void SetBall()
    {
        this.scale = this.m_ResetParams.GetPropertyWithDefault("scale", 1);
        this.ball.transform.localScale = new Vector3(this.scale, this.scale, this.scale);
    }

    public void SetResetParameters()
    {
        this.SetRacket();
        this.SetBall();
    }
}
