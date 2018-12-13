using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour {
    public float moveSpeed;
    private Transform tr;
    private Camera cam;

    public float minSize;
    public float maxSize;

    private void Start()
    {
        tr = GetComponent<Transform>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        cam.orthographicSize = minSize;
    }

    private void LateUpdate()
    {
        moveInputKey();
    }

    void moveInputKey()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            tr.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        else if (Input.GetKey(KeyCode.RightArrow))
            tr.Translate(Vector3.right * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.UpArrow))
            tr.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        else if (Input.GetKey(KeyCode.DownArrow))
            tr.Translate(Vector3.back * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Z))
        {
            cam.orthographicSize += 1.0f;
            if (cam.orthographicSize > maxSize)
                cam.orthographicSize = maxSize;

        }
        if (Input.GetKey(KeyCode.X))
        {
            cam.orthographicSize -= 1.0f;
            if (cam.orthographicSize < minSize)
                cam.orthographicSize = minSize;
        }

    }
}
