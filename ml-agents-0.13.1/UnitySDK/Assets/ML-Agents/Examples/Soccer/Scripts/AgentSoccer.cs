using UnityEngine;
using MLAgents;

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player
    public enum Team
    {
        Purple,
        Blue
    }
    public enum AgentRole
    {
        Striker,
        Goalie
    }

    public Team team;
    public AgentRole agentRole;
    float m_KickPower;
    int m_PlayerIndex;
    public SoccerFieldArea area;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerAcademy m_Academy;
    Renderer m_AgentRenderer;

    public void ChooseRandomTeam()
    {
        this.team = (Team)Random.Range(0, 2);
        if (this.team == Team.Purple)
        {
            this.JoinPurpleTeam(this.agentRole);
        }
        else
        {
            this.JoinBlueTeam(this.agentRole);
        }
    }

    public void JoinPurpleTeam(AgentRole role)
    {
        this.agentRole = role;
        this.team = Team.Purple;
        this.m_AgentRenderer.material = this.m_Academy.purpleMaterial;
        this.tag = "purpleAgent";
    }

    public void JoinBlueTeam(AgentRole role)
    {
        this.agentRole = role;
        this.team = Team.Blue;
        this.m_AgentRenderer.material = this.m_Academy.blueMaterial;
        this.tag = "blueAgent";
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.m_AgentRenderer = this.GetComponentInChildren<Renderer>();
        this.m_Academy = FindObjectOfType<SoccerAcademy>();
        this.agentRb = this.GetComponent<Rigidbody>();
        this.agentRb.maxAngularVelocity = 500;

        var playerState = new PlayerState
        {
            agentRb = agentRb,
            startingPos = this.transform.position,
            agentScript = this,
        };
        this.area.playerStates.Add(playerState);
        this.m_PlayerIndex = this.area.playerStates.IndexOf(playerState);
        playerState.playerIndex = this.m_PlayerIndex;
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);

        // Goalies and Strikers have slightly different action spaces.
        if (this.agentRole == AgentRole.Goalie)
        {
            this.m_KickPower = 0f;
            switch (action)
            {
                case 1:
                    dirToGo = this.transform.forward * 1f;
                    this.m_KickPower = 1f;
                    break;
                case 2:
                    dirToGo = this.transform.forward * -1f;
                    break;
                case 4:
                    dirToGo = this.transform.right * -1f;
                    break;
                case 3:
                    dirToGo = this.transform.right * 1f;
                    break;
            }
        }
        else
        {
            this.m_KickPower = 0f;
            switch (action)
            {
                case 1:
                    dirToGo = this.transform.forward * 1f;
                    this.m_KickPower = 1f;
                    break;
                case 2:
                    dirToGo = this.transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = this.transform.up * 1f;
                    break;
                case 4:
                    rotateDir = this.transform.up * -1f;
                    break;
                case 5:
                    dirToGo = this.transform.right * -0.75f;
                    break;
                case 6:
                    dirToGo = this.transform.right * 0.75f;
                    break;
            }
        }
        this.transform.Rotate(rotateDir, Time.deltaTime * 100f);
        this.agentRb.AddForce(dirToGo * this.m_Academy.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void AgentAction(float[] vectorAction)
    {
        // Existential penalty for strikers.
        if (this.agentRole == AgentRole.Striker)
        {
            this.AddReward(-1f / 3000f);
        }
        // Existential bonus for goalies.
        if (this.agentRole == AgentRole.Goalie)
        {
            this.AddReward(1f / 3000f);
        }
        this.MoveAgent(vectorAction);
    }

    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        var force = 2000f * this.m_KickPower;
        if (c.gameObject.CompareTag("ball"))
        {
            var dir = c.contacts[0].point - this.transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    public override void AgentReset()
    {
        if (this.m_Academy.randomizePlayersTeamForTraining)
        {
            this.ChooseRandomTeam();
        }

        if (this.team == Team.Purple)
        {
            this.JoinPurpleTeam(this.agentRole);
            this.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
        else
        {
            this.JoinBlueTeam(this.agentRole);
            this.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        this.transform.position = this.area.GetRandomSpawnPos(this.agentRole, this.team);
        this.agentRb.velocity = Vector3.zero;
        this.agentRb.angularVelocity = Vector3.zero;
        this.SetResetParameters();
    }

    public void SetResetParameters()
    {
        this.area.ResetBall();
    }
}
