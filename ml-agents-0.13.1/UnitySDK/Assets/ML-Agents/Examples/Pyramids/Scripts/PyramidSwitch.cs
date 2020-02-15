using UnityEngine;

public class PyramidSwitch : MonoBehaviour
{
    public Material onMaterial;
    public Material offMaterial;
    public GameObject myButton;
    bool m_State;
    GameObject m_Area;
    PyramidArea m_AreaComponent;
    int m_PyramidIndex;

    public bool GetState()
    {
        return this.m_State;
    }

    void Start()
    {
        this.m_Area = this.gameObject.transform.parent.gameObject;
        this.m_AreaComponent = this.m_Area.GetComponent<PyramidArea>();
    }

    public void ResetSwitch(int spawnAreaIndex, int pyramidSpawnIndex)
    {
        this.m_AreaComponent.PlaceObject(this.gameObject, spawnAreaIndex);
        this.m_State = false;
        this.m_PyramidIndex = pyramidSpawnIndex;
        this.tag = "switchOff";
        this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        this.myButton.GetComponent<Renderer>().material = this.offMaterial;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("agent") && this.m_State == false)
        {
            this.myButton.GetComponent<Renderer>().material = this.onMaterial;
            this.m_State = true;
            this.m_AreaComponent.CreatePyramid(1, this.m_PyramidIndex);
            this.tag = "switchOn";
        }
    }
}
