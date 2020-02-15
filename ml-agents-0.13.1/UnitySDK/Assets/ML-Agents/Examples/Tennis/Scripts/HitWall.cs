using UnityEngine;

public class HitWall : MonoBehaviour
{
    public GameObject areaObject;
    public int lastAgentHit;

    TennisArea m_Area;
    TennisAgent m_AgentA;
    TennisAgent m_AgentB;

    // Use this for initialization
    void Start()
    {
        this.m_Area = this.areaObject.GetComponent<TennisArea>();
        this.m_AgentA = this.m_Area.agentA.GetComponent<TennisAgent>();
        this.m_AgentB = this.m_Area.agentB.GetComponent<TennisAgent>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "over")
        {
            if (this.lastAgentHit == 0)
            {
                this.m_AgentA.AddReward(0.1f);
            }
            else
            {
                this.m_AgentB.AddReward(0.1f);
            }
            this.lastAgentHit = 0;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("iWall"))
        {
            if (collision.gameObject.name == "wallA")
            {
                if (this.lastAgentHit == 0)
                {
                    this.m_AgentA.AddReward(-0.01f);
                    this.m_AgentB.SetReward(0);
                    this.m_AgentB.score += 1;
                }
                else
                {
                    this.m_AgentA.SetReward(0);
                    this.m_AgentB.AddReward(-0.01f);
                    this.m_AgentA.score += 1;
                }
            }
            else if (collision.gameObject.name == "wallB")
            {
                if (this.lastAgentHit == 0)
                {
                    this.m_AgentA.AddReward(-0.01f);
                    this.m_AgentB.SetReward(0);
                    this.m_AgentB.score += 1;
                }
                else
                {
                    this.m_AgentA.SetReward(0);
                    this.m_AgentB.AddReward(-0.01f);
                    this.m_AgentA.score += 1;
                }
            }
            else if (collision.gameObject.name == "floorA")
            {
                if (this.lastAgentHit == 0 || this.lastAgentHit == -1)
                {
                    this.m_AgentA.AddReward(-0.01f);
                    this.m_AgentB.SetReward(0);
                    this.m_AgentB.score += 1;
                }
                else
                {
                    this.m_AgentA.AddReward(-0.01f);
                    this.m_AgentB.SetReward(0);
                    this.m_AgentB.score += 1;
                }
            }
            else if (collision.gameObject.name == "floorB")
            {
                if (this.lastAgentHit == 1 || this.lastAgentHit == -1)
                {
                    this.m_AgentA.SetReward(0);
                    this.m_AgentB.AddReward(-0.01f);
                    this.m_AgentA.score += 1;
                }
                else
                {
                    this.m_AgentA.SetReward(0);
                    this.m_AgentB.AddReward(-0.01f);
                    this.m_AgentA.score += 1;
                }
            }
            else if (collision.gameObject.name == "net")
            {
                if (this.lastAgentHit == 0)
                {
                    this.m_AgentA.AddReward(-0.01f);
                    this.m_AgentB.SetReward(0);
                    this.m_AgentB.score += 1;
                }
                else
                {
                    this.m_AgentA.SetReward(0);
                    this.m_AgentB.AddReward(-0.01f);
                    this.m_AgentA.score += 1;
                }
            }
            this.m_AgentA.Done();
            this.m_AgentB.Done();
            this.m_Area.MatchReset();
        }

        if (collision.gameObject.CompareTag("agent"))
        {
            this.lastAgentHit = collision.gameObject.name == "AgentA" ? 0 : 1;
        }
    }
}
