using UnityEngine;

namespace MLAgents
{
    public class FlyCamera : MonoBehaviour
    {
        /*
        wasd : basic movement
        shift : Makes camera accelerate
        space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/


        public float mainSpeed = 100.0f; // regular speed
        public float shiftAdd = 250.0f; // multiplied by how long shift is held.  Basically running
        public float maxShift = 1000.0f; // Maximum speed when holdin gshift
        public float camSens = 0.25f; // How sensitive it with mouse
        public bool rotateOnlyIfMousedown = true;
        public bool movementStaysFlat = true;

        Vector3
            m_LastMouse =
            new Vector3(255, 255,
                255);     // kind of in the middle of the screen, rather than at the top (play)

        float m_TotalRun = 1.0f;

        void Awake()
        {
            Debug.Log("FlyCamera Awake() - RESETTING CAMERA POSITION"); // nop?
            // nop:
            // transform.position.Set(0,8,-32);
            // transform.rotation.Set(15,0,0,1);
            this.transform.position = new Vector3(0, 8, -32);
            this.transform.rotation = Quaternion.Euler(25, 0, 0);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                this.m_LastMouse = Input.mousePosition; // $CTK reset when we begin
            }

            if (!this.rotateOnlyIfMousedown ||
                (this.rotateOnlyIfMousedown && Input.GetMouseButton(1)))
            {
                this.m_LastMouse = Input.mousePosition - this.m_LastMouse;
                this.m_LastMouse = new Vector3(-this.m_LastMouse.y * this.camSens, this.m_LastMouse.x * this.camSens, 0);
                this.m_LastMouse = new Vector3(this.transform.eulerAngles.x + this.m_LastMouse.x,
                    this.transform.eulerAngles.y + this.m_LastMouse.y, 0);
                this.transform.eulerAngles = this.m_LastMouse;
                this.m_LastMouse = Input.mousePosition;
                // Mouse  camera angle done.
            }

            // Keyboard commands
            var p = this.GetBaseInput();
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.m_TotalRun += Time.deltaTime;
                p = this.shiftAdd * this.m_TotalRun * p;
                p.x = Mathf.Clamp(p.x, -this.maxShift, this.maxShift);
                p.y = Mathf.Clamp(p.y, -this.maxShift, this.maxShift);
                p.z = Mathf.Clamp(p.z, -this.maxShift, this.maxShift);
            }
            else
            {
                this.m_TotalRun = Mathf.Clamp(this.m_TotalRun * 0.5f, 1f, 1000f);
                p = p * this.mainSpeed;
            }

            p = p * Time.deltaTime;
            var newPosition = this.transform.position;
            if (Input.GetKey(KeyCode.Space)
                || (this.movementStaysFlat && !(this.rotateOnlyIfMousedown && Input.GetMouseButton(1))))
            {
                // If player wants to move on X and Z axis only
                this.transform.Translate(p);
                newPosition.x = this.transform.position.x;
                newPosition.z = this.transform.position.z;
                this.transform.position = newPosition;
            }
            else
            {
                this.transform.Translate(p);
            }
        }

        Vector3 GetBaseInput()
        {
            // returns the basic values, if it's 0 than it's not active.
            var pVelocity = new Vector3();
            if (Input.GetKey(KeyCode.W))
            {
                pVelocity += new Vector3(0, 0, 1);
            }

            if (Input.GetKey(KeyCode.S))
            {
                pVelocity += new Vector3(0, 0, -1);
            }

            if (Input.GetKey(KeyCode.A))
            {
                pVelocity += new Vector3(-1, 0, 0);
            }

            if (Input.GetKey(KeyCode.D))
            {
                pVelocity += new Vector3(1, 0, 0);
            }

            return pVelocity;
        }
    }
}
