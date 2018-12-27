using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraCtrl : MonoBehaviour {
    public float moveSpeed;
    private Transform tr;
    private GameObject camGO;
    private Camera cam;
    private Transform playerTr;

    public float minSize;
    public float maxSize;

    public float zoomValue;
    private bool isZoomIn = false;
    private bool isZoomOut = false;
    private bool isLookAtPlayer = false;

    private void Start()
    {
        tr = GetComponent<Transform>();
        playerTr = GameObject.Find("Hero_0").GetComponent<Transform>();
        camGO = GameObject.FindGameObjectWithTag("MainCamera");
        cam = camGO.GetComponent<Camera>();
        cam.orthographicSize = maxSize / 2.0f;
    }

    private void LateUpdate()
    {
        #region moveSys
        if (isZoomIn) front();
        if (isZoomOut) back();
        if (isLookAtPlayer) LookAtP();
        #endregion
    } 

    public void moveToCam(int valud)
    {
        if (valud == 0) tr.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.Self);
        if (valud == 1) tr.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.Self);
        if (valud == 2) tr.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        if (valud == 3) tr.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.Self);
    }

    public void front()
    {
        cam.orthographicSize -= zoomValue * Time.deltaTime;
        if (cam.orthographicSize < minSize)
            cam.orthographicSize = minSize;
    }

    public void back()
    {
        cam.orthographicSize += zoomValue * Time.deltaTime;
        if (cam.orthographicSize > maxSize)
            cam.orthographicSize = maxSize;
    }

    public void LookAtP()
    {
        tr.position = Vector3.Lerp(tr.position , new Vector3(playerTr.position.x + -8.0f, 0, playerTr.position.z + -8.0f), Time.deltaTime);
    }

    public void isButtonD(string obj)
    {
        if (obj == "ZoomIn"){
            isZoomIn = true;
        }
        else if (obj == "ZoomOut")
        {
            isZoomOut = true;
        }else if (obj == "LookAtPlayer")
        {
            isLookAtPlayer = !isLookAtPlayer;
        }
    }

    public void isButtonU(string obj)
    {
        if (obj == "ZoomIn")
        {
            isZoomIn = false;
        }
        else if (obj == "ZoomOut")
        {
            isZoomOut = false;
        }
        else if (obj == "LookAtPlayer")
        {
            // Null
        }
    }
}
