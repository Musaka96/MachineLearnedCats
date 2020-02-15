using UnityEngine;
using MLAgents;

public class FoodCollectorArea : Area
{
    public GameObject food;
    public GameObject badFood;
    public int numFood;
    public int numBadFood;
    public bool respawnFood;
    public float range;

    void CreateFood(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject f = Instantiate(type, new Vector3(Random.Range(-this.range, this.range), 1f,
                Random.Range(-this.range, this.range)) + this.transform.position,
                Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 90f)));
            f.GetComponent<FoodLogic>().respawn = this.respawnFood;
            f.GetComponent<FoodLogic>().myArea = this;
        }
    }

    public void ResetFoodArea(GameObject[] agents)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == this.gameObject.transform)
            {
                agent.transform.position = new Vector3(Random.Range(-this.range, this.range), 2f,
                    Random.Range(-this.range, this.range))
                    + this.transform.position;
                agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
            }
        }

        this.CreateFood(this.numFood, this.food);
        this.CreateFood(this.numBadFood, this.badFood);
    }

    public override void ResetArea()
    {
    }
}
