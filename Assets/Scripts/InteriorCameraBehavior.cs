using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InteriorCameraBehavior : MonoBehaviour
{

    [System.NonSerialized]
    public float rotationY = 0.0f;

    [System.NonSerialized]
    public float rotationX = 0.0f;

    [System.NonSerialized]
    public float oldMouseX = 0.0f;

    [System.NonSerialized]
    public float oldMouseY = 0.0f;

    [System.NonSerialized]
    public bool dragging = false;

    
    enum DragMode
    {
        Camera,
        Object
    }

    private DragMode dragMode = DragMode.Camera;
    private GameObject dragginObject = null;

    private Camera currentCamera = null;

    // Use this for initialization
    void Start()
    {
        currentCamera = GameObject.FindObjectOfType<Camera>();
    }

    public Ray GetPointerRay()
    {
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        return ray;
    }

    public bool CheckShoudStart()
    {
        var shoudStart = false;
        var ray = GetPointerRay();
        List<RaycastResult> raycastResult = new List<RaycastResult>();
        if (EventSystem.current != null)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            var mp = Input.mousePosition;
            pointer.position = new Vector2(mp.x, mp.y);
            EventSystem.current.RaycastAll(pointer, raycastResult);
        }
        if (EventSystem.current == null || raycastResult.Count <= 0 || !raycastResult[0].gameObject.GetComponent<RectTransform>())
        {

            shoudStart = true;
        }
        return shoudStart;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            var ray = GetPointerRay();

            dragMode = DragMode.Camera;

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.collider.name == "Object")
                {
                    dragMode = DragMode.Object;
                    dragginObject = hitInfo.collider.gameObject;
                }
            }


            dragging = true;
            oldMouseX = Input.mousePosition.x;
            oldMouseY = Input.mousePosition.y;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            if (dragMode == DragMode.Camera)
            {
                float mouseMoveSize = 1000.0f / Screen.width;
                var mouseX = Input.mousePosition.x;
                var mouseY = Input.mousePosition.y;
                float dry = (mouseX - oldMouseX) * mouseMoveSize;
                float drx = (mouseY - oldMouseY) * mouseMoveSize;
                oldMouseX = mouseX;
                oldMouseY = mouseY;

                rotationX -= drx;
                rotationY += dry;

                if (rotationX < -90)
                {
                    rotationX = -90;
                }

                if (rotationX > 90)
                {
                    rotationX = 90;
                }

                var q = Quaternion.Euler(new Vector3(0, rotationY, 0)) * Quaternion.Euler(new Vector3(rotationX, 0, 0));
                currentCamera.transform.localRotation = q;
            }
            else if (dragMode == DragMode.Object)
            {
				float mouseMoveSize = 2.0f / Screen.width;
                var mouseX = Input.mousePosition.x;
                var mouseY = Input.mousePosition.y;
                float drx = (mouseX - oldMouseX) * mouseMoveSize;
                float dry = (mouseY - oldMouseY) * mouseMoveSize;
                oldMouseX = mouseX;
                oldMouseY = mouseY;

                var forward = currentCamera.transform.forward;
                forward.y = 0;
                forward.Normalize();
                var right = currentCamera.transform.right;
                right.y = 0;
                right.Normalize();

                dragginObject.transform.localPosition += forward * dry + right * drx;
            }
           
        }
    }
}
