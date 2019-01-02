using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletSys : MonoBehaviour
{
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