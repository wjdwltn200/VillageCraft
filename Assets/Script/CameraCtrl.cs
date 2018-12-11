using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour {

    public float moveSpeed;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Translate(moveSpeed * Time.deltaTime, 0, 0, Space.World);
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Translate(moveSpeed * Time.deltaTime, 0, 0, Space.World);
    }
}
