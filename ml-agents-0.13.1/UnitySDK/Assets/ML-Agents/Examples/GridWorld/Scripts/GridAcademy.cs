using UnityEngine;
using MLAgents;

public class GridAcademy : Academy
{
    public Camera MainCamera;

    public override void InitializeAcademy()
    {
        this.FloatProperties.RegisterCallback("gridSize", f =>
        {
            this.MainCamera.transform.position = new Vector3(-(f - 1) / 2f, f * 1.25f, -(f - 1) / 2f);
            this.MainCamera.orthographicSize = (f + 5f) / 2f;
        });

    }
}
