using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class FoodCollectorAcademy : Academy
{
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public FoodCollectorArea[] listArea;

    public int totalScore;
    public Text scoreText;
    public override void AcademyReset()
    {
        this.ClearObjects(GameObject.FindGameObjectsWithTag("food"));
        this.ClearObjects(GameObject.FindGameObjectsWithTag("badFood"));

        this.agents = GameObject.FindGameObjectsWithTag("agent");
        this.listArea = FindObjectsOfType<FoodCollectorArea>();
        foreach (var fa in this.listArea)
        {
            fa.ResetFoodArea(this.agents);
        }

        this.totalScore = 0;
    }

    void ClearObjects(GameObject[] objects)
    {
        foreach (var food in objects)
        {
            Destroy(food);
        }
    }

    public override void AcademyStep()
    {
        this.scoreText.text = string.Format(@"Score: {0}", this.totalScore);
    }
}
