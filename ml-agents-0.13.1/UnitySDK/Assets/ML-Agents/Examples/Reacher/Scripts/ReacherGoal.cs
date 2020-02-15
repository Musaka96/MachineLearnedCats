using UnityEngine;

public class ReacherGoal : MonoBehaviour
{
    public GameObject agent;
    public GameObject hand;
    public GameObject goalOn;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == this.hand)
        {
            this.goalOn.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == this.hand)
        {
            this.goalOn.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == this.hand)
        {
            this.agent.GetComponent<ReacherAgent>().AddReward(0.01f);
        }
    }
}
