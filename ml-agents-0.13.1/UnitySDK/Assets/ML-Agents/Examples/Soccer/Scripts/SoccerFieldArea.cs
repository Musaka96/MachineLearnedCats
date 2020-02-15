using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerState
{
    public int playerIndex;
    [FormerlySerializedAs("agentRB")]
    public Rigidbody agentRb;
    public Vector3 startingPos;
    public AgentSoccer agentScript;
    public float ballPosReward;
}

public class SoccerFieldArea : MonoBehaviour
{
    public GameObject ball;
    [FormerlySerializedAs("ballRB")]
    [HideInInspector]
    public Rigidbody ballRb;
    public GameObject ground;
    public GameObject centerPitch;
    SoccerBallController m_BallController;
    public List<PlayerState> playerStates = new List<PlayerState>();
    [HideInInspector]
    public Vector3 ballStartingPos;
    public GameObject goalTextUI;
    [HideInInspector]
    public bool canResetBall;
    Material m_GroundMaterial;
    Renderer m_GroundRenderer;
    SoccerAcademy m_Academy;

    public IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        this.m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        this.m_GroundRenderer.material = this.m_GroundMaterial;
    }

    void Awake()
    {
        this.m_Academy = FindObjectOfType<SoccerAcademy>();
        this.m_GroundRenderer = this.centerPitch.GetComponent<Renderer>();
        this.m_GroundMaterial = this.m_GroundRenderer.material;
        this.canResetBall = true;
        if (this.goalTextUI) { this.goalTextUI.SetActive(false); }
        this.ballRb = this.ball.GetComponent<Rigidbody>();
        this.m_BallController = this.ball.GetComponent<SoccerBallController>();
        this.m_BallController.area = this;
        this.ballStartingPos = this.ball.transform.position;
    }

    IEnumerator ShowGoalUI()
    {
        if (this.goalTextUI) this.goalTextUI.SetActive(true);
        yield return new WaitForSeconds(.25f);
        if (this.goalTextUI) this.goalTextUI.SetActive(false);
    }

    public void AllPlayersDone(float reward)
    {
        foreach (var ps in this.playerStates)
        {
            if (ps.agentScript.gameObject.activeInHierarchy)
            {
                if (reward != 0)
                {
                    ps.agentScript.AddReward(reward);
                }
                ps.agentScript.Done();
            }
        }
    }

    public void GoalTouched(AgentSoccer.Team scoredTeam)
    {
        foreach (var ps in this.playerStates)
        {
            if (ps.agentScript.team == scoredTeam)
            {
                this.RewardOrPunishPlayer(ps, this.m_Academy.strikerReward, this.m_Academy.goalieReward);
            }
            else
            {
                this.RewardOrPunishPlayer(ps, this.m_Academy.strikerPunish, this.m_Academy.goaliePunish);
            }
            if (this.m_Academy.randomizePlayersTeamForTraining)
            {
                ps.agentScript.ChooseRandomTeam();
            }

            if (scoredTeam == AgentSoccer.Team.Purple)
            {
                this.StartCoroutine(this.GoalScoredSwapGroundMaterial(this.m_Academy.purpleMaterial, 1));
            }
            else
            {
                this.StartCoroutine(this.GoalScoredSwapGroundMaterial(this.m_Academy.blueMaterial, 1));
            }
            if (this.goalTextUI)
            {
                this.StartCoroutine(this.ShowGoalUI());
            }
        }
    }

    public void RewardOrPunishPlayer(PlayerState ps, float striker, float goalie)
    {
        if (ps.agentScript.agentRole == AgentSoccer.AgentRole.Striker)
        {
            ps.agentScript.AddReward(striker);
        }
        if (ps.agentScript.agentRole == AgentSoccer.AgentRole.Goalie)
        {
            ps.agentScript.AddReward(goalie);
        }
        ps.agentScript.Done();  //all agents need to be reset
    }

    public Vector3 GetRandomSpawnPos(AgentSoccer.AgentRole role, AgentSoccer.Team team)
    {
        var xOffset = 0f;
        if (role == AgentSoccer.AgentRole.Goalie)
        {
            xOffset = 13f;
        }
        if (role == AgentSoccer.AgentRole.Striker)
        {
            xOffset = 7f;
        }
        if (team == AgentSoccer.Team.Blue)
        {
            xOffset = xOffset * -1f;
        }
        var randomSpawnPos = this.ground.transform.position +
            new Vector3(xOffset, 0f, 0f)
            + (Random.insideUnitSphere * 2);
        randomSpawnPos.y = this.ground.transform.position.y + 2;
        return randomSpawnPos;
    }

    public Vector3 GetBallSpawnPosition()
    {
        var randomSpawnPos = this.ground.transform.position +
            new Vector3(0f, 0f, 0f)
            + (Random.insideUnitSphere * 2);
        randomSpawnPos.y = this.ground.transform.position.y + 2;
        return randomSpawnPos;
    }

    public void ResetBall()
    {
        this.ball.transform.position = this.GetBallSpawnPosition();
        this.ballRb.velocity = Vector3.zero;
        this.ballRb.angularVelocity = Vector3.zero;

        var ballScale = this.m_Academy.FloatProperties.GetPropertyWithDefault("ball_scale", 0.015f);
        this.ballRb.transform.localScale = new Vector3(ballScale, ballScale, ballScale);
    }
}
