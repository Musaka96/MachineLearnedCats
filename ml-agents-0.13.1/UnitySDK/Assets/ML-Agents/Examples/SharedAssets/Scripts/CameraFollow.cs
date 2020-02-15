using UnityEngine;

namespace MLAgents
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        Vector3 m_Offset;

        // Use this for initialization
        void Start()
        {
            this.m_Offset = this.gameObject.transform.position - this.target.position;
        }

        // Update is called once per frame
        void Update()
        {
            // gameObject.transform.position = target.position + offset;
            var newPosition = new Vector3(this.target.position.x + this.m_Offset.x, this.transform.position.y,
                this.target.position.z + this.m_Offset.z);
            this.gameObject.transform.position = newPosition;
        }
    }
}
