using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowCtrl : MonoBehaviour {
    public Transform startPos;
    public Transform endPos;
    public float maxHeight = 5.0f;
    public float totalTime = 10.0f;

    // >> 높이 관련
    float power = 100.0f;
    float currPower = 0.0f;
    float gravity = 0.0f;
    float elapsedTime_Y = 0.0f;
    // << 높이 관련

    // >> xz 이동 관련
    Vector3 moveDir;
    float moveSpeed;
    float elapsedTime_XZ = 0.0f;
    // << xz 이동 관련

    // Use this for initialization
    void Start () {
        transform.position = startPos.position;

        // >> 높이 관련
        currPower = power;

        // 1) maxHeight + startPos.position.y = startPos.position.y + power * totalTime + (gravity * totalTime * totalTime) * 0.5f;
        // 2) (gravity * totalTime * totalTime * 0.5f = maxHeight - (power * totalTime);
        //gravity = (maxHeight - (power * totalTime)) * 2.0f / totalTime / totalTime;

        gravity = -power / totalTime;

        Debug.Log("1. gravity : " + gravity + ", maxHeight : " + maxHeight);

        float height = startPos.position.y + (power * totalTime) + (0.5f * gravity * totalTime * totalTime);
        Debug.Log("1. height : " + height);

        // 비율에 맞춘 초기 스피드 height: power = maxHeight : x;
        power = power * maxHeight / height;

        gravity = -power / totalTime;

        Debug.Log("2. gravity : " + gravity + ", maxHeight : " + maxHeight);

        height = startPos.position.y + (power * totalTime) + (0.5f * gravity * totalTime * totalTime);
        Debug.Log("2. height : " + height);

        elapsedTime_Y = 0.0f;
        // << 높이 관련

        // >> xz 이동 관련
        moveDir = (endPos.position - startPos.position).normalized;
        moveSpeed = (endPos.position - startPos.position).magnitude / (totalTime * 2.0f);

        elapsedTime_XZ = 0.0f;
        // << xz 이동 관련
    }

    // Update is called once per frame
    void Update ()
    {
        // >> xz 이동 관련
        if (elapsedTime_XZ <= totalTime * 2.0f)
        {
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            elapsedTime_XZ += Time.deltaTime;
        }
        // << xz 이동 관련
    }

    private void FixedUpdate()
    {
        // >> 높이 관련
        if (elapsedTime_Y <= totalTime * 2.0f)
        {
            elapsedTime_Y += Time.fixedDeltaTime;

            // 1) v = v0 + at
            currPower = power + gravity * elapsedTime_Y;

            // 2) v = v0 + 1/2(at^2)
            //currPower = power + gravity * elapsedTime_Y * elapsedTime_Y * 0.5f;

            Debug.Log("currPower : " + currPower + ", y : " + transform.position.y);
            transform.position += Vector3.up * currPower * Time.fixedDeltaTime;
            //Debug.Log("currPower * Time.fixedDeltaTime : " + currPower * Time.fixedDeltaTime);

            // >> axis, angle
            Vector3 axis = Vector3.Cross(moveDir, Vector3.up);
            float angle = 45 * currPower / power;
            transform.rotation = Quaternion.AngleAxis(angle, axis);
            // << axis, angle
        }
        // << 높이 관련
        
    }
}
