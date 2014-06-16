using UnityEngine;
using System.Collections;
using CivGrid;

namespace CivGrid
{

    public class CivGridCamera : MonoBehaviour
    {
        //Camera Settings
        public bool enableWrapping;
        [HideInInspector]
        public Camera cam1;
        public float cameraHeight = 3f;
        public float cameraAngle = 65f;
        public float cameraSpeed = 2f;

        private Vector2 camOffset;
        [HideInInspector]
        public Camera cam2;
        private Transform cam1T;
        private Transform cam2T;
        private bool cam1Lead;

        private Vector3 moveVector;
        private Vector3 pos;

        private WorldManager worldManager;

        //set up camera for wrapping/general use(move to seperate script soon)
        public void SetupCameras(WorldManager manager)
        {
            cam1 = this.gameObject.GetComponent<Camera>();
            this.worldManager = manager;
            if (enableWrapping == true)
            {
                this.worldManager.keepSymmetrical = false;
            }
            else
            {
                this.worldManager.keepSymmetrical = true; Debug.Log("is this expected?");
            }

            cam1.transform.position = new Vector3(0, 0, 0);
            //setup wrapping camera
            if (enableWrapping == true)
            {
                cam2 = new GameObject("Cam2").gameObject.AddComponent<Camera>();
                cam2.gameObject.AddComponent<GUILayer>();
                cam2.fieldOfView = cam1.fieldOfView;
                cam2.clearFlags = CameraClearFlags.Depth;
                cam2.depth = cam1.depth + 1;

                camOffset = new Vector2((this.worldManager.mapSize.x * this.worldManager.hexSize.x), 0);
            }

            cam1T = cam1.transform;
            cam1T.localEulerAngles = new Vector3(cameraAngle, cam1T.localEulerAngles.y, cam1T.localEulerAngles.z);
            cam1T.position = new Vector3(camOffset.x * .5f, cameraHeight, (manager.hexExt.y * manager.mapSize.y) * .5f);

            if (enableWrapping == true)
            {
                cam2T = cam2.transform;
                cam2T.position = cam1T.position;
                cam2T.rotation = cam1T.rotation;
            }
        }

        void Update()
        {
            CheckInput();
            if (enableWrapping)
            {
                UpdateCameraW();
            }
            else
            {
                UpdateCamera();
            }
        }

        void CheckInput()
        {
            if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus)) { moveVector.y = -1; }
            else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) { moveVector.y = 1; }
            else { moveVector.y = 0; }
        }

        void UpdateCamera()
        {
            Vector3 pos = cam1.ScreenToViewportPoint(worldManager.mousePos);

            //x vals
            if (pos.x >= 1)
            {
                pos.x = 1;
                moveVector.x = 1;
            }
            else if (pos.x <= 0)
            {
                pos.x = 0;
                moveVector.x = -1;
            }
            else
            {
                moveVector.x = 0;
            }

            //y vals
            if (pos.y >= 1)
            {
                pos.y = 1;
                moveVector.z = 1;
            }
            else if (pos.y <= 0)
            {
                pos.y = 0;
                moveVector.z = -1;
            }
            else
            {
                moveVector.z = 0;
            }

            cam1T.Translate(moveVector * (cameraSpeed * Time.deltaTime), Space.World);
        }

        private void MoveRightW()
        {
            pos.x = 1;
            moveVector.x = 1;
            if (cam1T.position.x > cam2T.position.x)
            {
                cam1Lead = true;
            }
            else { cam1Lead = false; }

            if (cam1T.position.x >= (worldManager.mapSize.x * worldManager.hexExt.x) * 2.98f)
            {
                cam1T.position = new Vector3(cam2T.position.x - camOffset.x, cam1T.position.y, cam1T.position.z);
                cam1Lead = false;
            }
            if (cam2T.position.x >= (worldManager.mapSize.x * worldManager.hexExt.x) * 2.98f)
            {
                cam2T.position = new Vector3(cam1T.position.x - camOffset.x, cam2T.position.y, cam2T.position.z);
                cam1Lead = true;
            }
            if (cam1Lead)
            {
                cam2T.position = new Vector3(cam1T.position.x - camOffset.x + 0.1f, cam2T.position.y, cam2T.position.z);
            }
            else
            {
                cam2T.position = new Vector3(cam1T.position.x + camOffset.x, cam2T.position.y, cam2T.position.z);
            }
        }

        private void MoveLeftW()
        {
            pos.x = 0;
            moveVector.x = -1;
            if (cam1T.position.x > cam2T.position.x)
            {
                cam1Lead = false;
            }
            else
            {
                cam1Lead = true;
            }
            if (cam1T.position.x <= -((worldManager.mapSize.x * worldManager.hexExt.x) * .98))
            {
                cam1T.position = new Vector3(cam2T.position.x + camOffset.x, cam1T.position.y, cam1T.position.z);
                cam1Lead = false;
            }
            if (cam2T.position.x <= -((worldManager.mapSize.x * worldManager.hexExt.x) * .98))
            {
                cam2T.position = new Vector3(cam1T.position.x + camOffset.x, cam2T.position.y, cam2T.position.z);
                cam1Lead = true;
            }
            if (cam1Lead)
            {
                cam2T.position = new Vector3(cam1T.position.x + camOffset.x - 0.1f, cam2T.position.y, cam2T.position.z);
            }
            else
            {
                cam2T.position = new Vector3(cam1T.position.x - camOffset.x, cam2T.position.y, cam2T.position.z);
            }
        }

        void UpdateCameraW()
        {
            pos = cam1.ScreenToViewportPoint(worldManager.mousePos);
            if (pos.x >= 0.8)
            {
                MoveRightW();
            }
            else if (pos.x <= 0.2)
            {
                MoveLeftW();
            }
            else
            {
                moveVector.x = 0;
            }

            if (pos.y >= 0.8)
            {
                pos.y = 1;
                moveVector.z = 1;
            }
            else if (pos.y <= 0.2)
            {
                pos.y = 0;
                moveVector.z = -1;
            }
            else
            {
                moveVector.z = 0;
            }
            cam1T.Translate(moveVector * (cameraSpeed * Time.deltaTime), Space.World);
            cam2T.position = new Vector3(cam2T.position.x, cam1T.position.y, cam1T.position.z);
        }
    }
}