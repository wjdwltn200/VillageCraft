using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsPoolMgr : MonoBehaviour {

    public List<GameObject> aliveListMons;

    private void Awake()
    {
        aliveListMons = new List<GameObject>();
    }

    public void addMons(GameObject Go, Vector3 pos)
    {
        GameObject tempGo;
        if (aliveListMons.Count > 0)
        {
            for (int i = 0; i < aliveListMons.Count; i++)
            {
                if (aliveListMons[i].activeSelf) continue;
                tempGo = aliveListMons[i];
                tempGo.SetActive(true);
                tempGo.transform.position = pos;
                tempGo.transform.Translate(Random.Range(-2.0f, 2.0f), 0, Random.Range(-2.0f, 2.0f));
                return;
            }
        }

        tempGo = Instantiate(Go, pos, transform.rotation, transform);
        tempGo.transform.position = pos;
        tempGo.transform.Translate(Random.Range(-2.0f, 2.0f), 0, Random.Range(-2.0f, 2.0f));
        aliveListMons.Add(tempGo);
        return;
    }
}
