using UnityEngine;

public class TennisArea : MonoBehaviour
{
    public GameObject ball;
    public GameObject agentA;
    public GameObject agentB;
    Rigidbody m_BallRb;

    // Use this for initialization
    void Start()
    {
        this.m_BallRb = this.ball.GetComponent<Rigidbody>();
        this.MatchReset();
    }

    public void MatchReset()
    {
        var ballOut = Random.Range(6f, 8f);
        var flip = Random.Range(0, 2);
        if (flip == 0)
        {
            this.ball.transform.position = new Vector3(-ballOut, 6f, 0f) + this.transform.position;
        }
        else
        {
            this.ball.transform.position = new Vector3(ballOut, 6f, 0f) + this.transform.position;
        }
        this.m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        this.ball.transform.localScale = new Vector3(1, 1, 1);
        this.ball.GetComponent<HitWall>().lastAgentHit = -1;
    }

    void FixedUpdate()
    {
        var rgV = this.m_BallRb.velocity;
        this.m_BallRb.velocity = new Vector3(Mathf.Clamp(rgV.x, -9f, 9f), Mathf.Clamp(rgV.y, -9f, 9f), rgV.z);
    }
}
