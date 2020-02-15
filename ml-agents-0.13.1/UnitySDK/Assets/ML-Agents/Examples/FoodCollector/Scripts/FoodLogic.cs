using UnityEngine;

public class FoodLogic : MonoBehaviour
{
    public bool respawn;
    public FoodCollectorArea myArea;

    public void OnEaten()
    {
        if (this.respawn)
        {
            this.transform.position = new Vector3(Random.Range(-this.myArea.range, this.myArea.range),
                3f,
                Random.Range(-this.myArea.range, this.myArea.range)) + this.myArea.transform.position;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
