using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSys : MonoBehaviour {

    public GameObject currTarget;
    private Transform tr;
    private BuildingData buildingData;

    private void Awake()
    {
        tr = GetComponent<Transform>();
        buildingData = GetComponent<BuildingData>();
    }

    private void Update()
    {
        if (currTarget != null)
            isTarget();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(currTarget == null && other.gameObject.layer == 9)
        {
            currTarget = other.gameObject;
        }
    }

    void isTarget()
    {
        if (Vector3.Distance(tr.position, currTarget.transform.position) <= buildingData.atkRange)
        {
            currTarget = null;
        }
    }
}
