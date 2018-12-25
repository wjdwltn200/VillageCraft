using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletSys : MonoBehaviour
{
    //private Vector3 vStart;
    //private Vector3 vEnd;

    //public float maxHeight = 5.0f;
    //public float totalTime = 10.0f;

    //// >> 높이 관련
    //float power = 100.0f;
    //float currPower = 0.0f;
    //float gravity = 0.0f;
    //float elapsedTime_Y = 0.0f;
    //// << 높이 관련

    //// >> xz 이동 관련
    //Vector3 moveDir;
    //float moveSpeed;
    //float elapsedTime_XZ = 0.0f;
    //// << xz 이동 관련

    //private float dam;

    //private Transform tr;
    //private Vector3 lookAtVec3;
    //private bool isDie;

    //private bulletMgr bulletMgr;

    //public void shotSetting(Vector3 start, Vector3 end, float rangeforce, float damge, bulletMgr mgr)
    //{
    //    bulletMgr = mgr;
    //    dam = damge;
    //    isDie = false;
    //    tr = GetComponent<Transform>();
    //    vStart = start;
    //    vEnd = end;

    //    transform.position = start;

    //    // >> 높이 관련
    //    currPower = power;

    //    // 1) maxHeight + startPos.position.y = startPos.position.y + power * totalTime + (gravity * totalTime * totalTime) * 0.5f;
    //    // 2) (gravity * totalTime * totalTime * 0.5f = maxHeight - (power * totalTime);
    //    //gravity = (maxHeight - (power * totalTime)) * 2.0f / totalTime / totalTime;

    //    gravity = -power / totalTime;

    //    Debug.Log("1. gravity : " + gravity + ", maxHeight : " + maxHeight);

    //    float height = start.y + (power * totalTime) + (0.5f * gravity * totalTime * totalTime);
    //    Debug.Log("1. height : " + height);

    //    // 비율에 맞춘 초기 스피드 height: power = maxHeight : x;
    //    power = power * maxHeight / height;

    //    gravity = -power / totalTime;

    //    Debug.Log("2. gravity : " + gravity + ", maxHeight : " + maxHeight);

    //    height = start.y + (power * totalTime) + (0.5f * gravity * totalTime * totalTime);
    //    Debug.Log("2. height : " + height);

    //    elapsedTime_Y = 0.0f;
    //    // << 높이 관련

    //    // >> xz 이동 관련
    //    moveDir = (end - start).normalized;
    //    moveSpeed = (end - start).magnitude / (totalTime * 2.0f);

    //    elapsedTime_XZ = 0.0f;
    //    // << xz 이동 관련

    //}

    //void Update()
    //{
    //    if (isDie) return;

    //    // >> xz 이동 관련
    //    if (elapsedTime_XZ <= totalTime * 2.0f)
    //    {
    //        transform.position += moveDir * moveSpeed * Time.deltaTime;

    //        elapsedTime_XZ += Time.deltaTime;
    //    }
    //    // << xz 이동 관련


    //    if (tr.position.y <= vEnd.y)
    //    {
    //        isDie = true;
    //        bulletMgr.addDieList(gameObject);
    //        gameObject.SetActive(false);
    //    }
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!isDie && other.gameObject.layer == 9)
    //    {
    //        other.gameObject.GetComponent<damageSys>().setHpPoint(dam);
    //        isDie = true;
    //        bulletMgr.addDieList(gameObject);
    //        gameObject.SetActive(false);

    //    }
    //}

    //private void FixedUpdate()
    //{
    //    // >> 높이 관련
    //    if (elapsedTime_Y <= totalTime * 2.0f)
    //    {
    //        elapsedTime_Y += Time.fixedDeltaTime;

    //        // 1) v = v0 + at
    //        currPower = power + gravity * elapsedTime_Y;

    //        // 2) v = v0 + 1/2(at^2)
    //        //currPower = power + gravity * elapsedTime_Y * elapsedTime_Y * 0.5f;

    //        transform.position += Vector3.up * currPower * Time.fixedDeltaTime;
    //        //Debug.Log("currPower * Time.fixedDeltaTime : " + currPower * Time.fixedDeltaTime);

    //        // >> axis, angle
    //        Vector3 axis = Vector3.Cross(moveDir, Vector3.up);
    //        float angle = 45 * currPower / power;
    //        transform.rotation = Quaternion.AngleAxis(angle, axis);
    //        // << axis, angle
    //    }
    //    // << 높이 관련

    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(vEnd, 0.5f);

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawSphere(lookAtVec3, 0.5f);
    //}

    private Vector3 vStart;
    private Vector3 vEnd;
    private Transform tr;

    public float height;
    private float currHeight;
    public float speedPower;
    private float currSpeedPower;
    private float dam;
    private float gr = 9.81f;


    private Vector3 lookAtVec3;
    private bool isTop;
    private bool isDie;

    private bulletMgr bulletMgr;

    public void shotSetting(Vector3 start, Vector3 end, float rangeforce, float damge, bulletMgr mgr)
    {
        bulletMgr = mgr;
        currHeight = height;
        vStart = tr.transform.position = start;
        vEnd = end;
        vEnd.x += (Random.Range(-rangeforce, +rangeforce));
        vEnd.z += (Random.Range(-rangeforce, +rangeforce));
        dam = damge;

        lookAtVec3 = Vector3.Lerp(vStart, vEnd, 0.3f);
        lookAtVec3.y = currHeight += vStart.y;
        isTop = false;
        currSpeedPower = speedPower;
        tr.LookAt(lookAtVec3);
        isDie = false;
    }

    private void Awake()
    {
        tr = GetComponent<Transform>();
    }

    void Update()
    {
        if (isDie) return;
        tr.LookAt(lookAtVec3);

        if (!isTop && tr.transform.position.y >= (currHeight * 0.8f))
        {
            isTop = true;
            lookAtVec3 = Vector3.Lerp(vStart, vEnd, 1.0f);
            lookAtVec3.y = currHeight + (height * 0.5f);
        }

        if (!isTop)
        {
            currSpeedPower -= gr * Time.deltaTime;
            tr.Translate(Vector3.forward * currSpeedPower * Time.deltaTime, Space.Self);
        }
        else
        {
            currSpeedPower += gr * Time.deltaTime;

            tr.Translate(Vector3.forward * currSpeedPower * Time.deltaTime, Space.Self);
            if (lookAtVec3.y > vEnd.y)
                lookAtVec3.y -= (gr + currSpeedPower) * Time.deltaTime;
        }

        tr.Translate(Vector3.up * currHeight * Time.deltaTime, Space.World);


        if (tr.position.y <= vEnd.y)
        {
            isDie = true;
            bulletMgr.addDieList(gameObject);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDie && other.gameObject.layer == 9)
        {
            other.gameObject.GetComponent<damageSys>().setHpPoint(dam);
            isDie = true;
            bulletMgr.addDieList(gameObject);
            gameObject.SetActive(false);

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(vEnd, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(lookAtVec3, 0.5f);
    }
}