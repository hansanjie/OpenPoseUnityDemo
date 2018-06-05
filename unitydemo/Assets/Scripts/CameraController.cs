using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class CameraController : MonoBehaviour
    {
        // Singleton
        private static CameraController instance;

        //public float speed = 1f;
        //[SerializeField] KeyCode ForwardKey = KeyCode.W;
        //[SerializeField] KeyCode BackwardKey = KeyCode.S;
        //[SerializeField] KeyCode LeftKey = KeyCode.A;
        //[SerializeField] KeyCode RightKey = KeyCode.D;
        [SerializeField] Transform CameraCenter;
        [SerializeField] Transform CameraJoint;
        [SerializeField] Transform MyCamera;

        public Transform HumanCenter;

        // Camera control
        public float ControlDistanceSpeed = 1.1f;
        public float ControlRotateSpeed = 240f;
        public float MinDistance = 1.0f;
        public Vector2 ControlRotateLimit = new Vector2(0f, 60f);

        public float FollowRotatingCoeff = 0.5f;
        public float FollowTranslatingCoeff = 0.3f;
        //public float RotatingStiff = 0.5f;
        //public float TranslatingStiff = 0.5f;
        private float DefaultDistance;
        private Quaternion DefaultCenterRotation;
        private float InitDistance;
        //private Vector3 InitPos;
        //private Quaternion InitRotation;
        private Vector3 GoalWatch;
        private Vector3 GoalPos;

        public static Transform FocusCenter { set { instance.HumanCenter = value; } get { return instance.HumanCenter; } }
        public static Vector3 CameraJointPosition { set { instance.CameraJoint.position = value; } get { return instance.CameraJoint.position; } }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            DefaultDistance = Vector3.Dot(GoalPos - MyCamera.position, MyCamera.forward);
            DefaultCenterRotation = CameraCenter.localRotation;
            InitDistance = DefaultDistance;
            //InitPos = MyCamera.position;
            //InitRotation = MyCamera.rotation;
            
        }

        public void SetToDefault()
        {
            InitDistance = DefaultDistance;
            CameraCenter.localRotation = DefaultCenterRotation;
        }

        private void SetGoalUpdate()
        {
            GoalWatch = HumanCenter.position;
            GoalPos = HumanCenter.position;
        }

        private void MoveToGoalUpdate()
        {
            // Rotation
            float angleDiff;
            Vector3 axisRotate;
            Quaternion.FromToRotation(CameraJoint.forward, GoalWatch - CameraJoint.position).ToAngleAxis(out angleDiff, out axisRotate);
            //float deltaAngle = FollowRotatingCoeff * Mathf.Sqrt(angleDiff) * Time.deltaTime; // index 1/2
            float deltaAngle = FollowRotatingCoeff * angleDiff * Mathf.Abs(angleDiff) * Time.deltaTime; // index 2
            CameraJoint.Rotate(axisRotate, deltaAngle, Space.World);

            // Translation
            float distanceDiff = Vector3.Dot(GoalPos - MyCamera.position, MyCamera.forward) - InitDistance;
            //float deltaMove = FollowTranslatingCoeff * distanceDiff * Time.deltaTime; // index 1
            float deltaMove = FollowTranslatingCoeff * distanceDiff * Mathf.Abs(distanceDiff) * Time.deltaTime; // index 2
            MyCamera.localPosition += new Vector3(0, 0, deltaMove);
            if (MyCamera.localPosition.z < 0f) MyCamera.localPosition = new Vector3(0f, 0f, 0f);
        }

        /*private void KeyboardControlUpdate()
        {
            Vector3 move = new Vector3();
            if (Input.GetKey(ForwardKey)) move += Time.deltaTime * speed * Vector3.back;
            if (Input.GetKey(BackwardKey)) move += Time.deltaTime * speed * Vector3.forward;
            if (Input.GetKey(LeftKey)) move += Time.deltaTime * speed * Vector3.right;
            if (Input.GetKey(RightKey)) move += Time.deltaTime * speed * Vector3.left;

            CameraJoint.Translate(move, Space.World);
        }*/

        private void MouseControlUpdate()
        {
            // Mouse input
            Vector2 deltaScreenPos = new Vector2();
            float deltaScroll = Input.GetAxis("Mouse ScrollWheel");
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                deltaScreenPos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }

            // Control camera
            Vector3 euler = CameraCenter.localRotation.eulerAngles + new Vector3(-deltaScreenPos.y, deltaScreenPos.x, 0f) * ControlRotateSpeed * Time.deltaTime;
            if (euler.x < ControlRotateLimit.x) euler.x = ControlRotateLimit.x;
            else if (euler.x > ControlRotateLimit.y) euler.x = ControlRotateLimit.y;
            CameraCenter.localRotation = Quaternion.Euler(euler);
            if (deltaScroll > 0) InitDistance /= ControlDistanceSpeed;
            else if (deltaScroll < 0) InitDistance *= ControlDistanceSpeed;
            if (InitDistance < MinDistance) InitDistance *= ControlDistanceSpeed;
            if (InitDistance * InitDistance > (CameraJoint.position - HumanCenter.position).sqrMagnitude) InitDistance /= ControlDistanceSpeed;
        }

        void Update()
        {
            MouseControlUpdate();
            SetGoalUpdate();
            MoveToGoalUpdate();
        }
    }
}
