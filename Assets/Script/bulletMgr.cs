using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMgr : MonoBehaviour
{
    //public List<GameObject> listBullet;
    public LinkedList<GameObject> aliveListBullet;
    public LinkedList<GameObject> dieListBullet;

    private void Awake()
    {
        aliveListBullet = new LinkedList<GameObject>();
        dieListBullet = new LinkedList<GameObject>();
    }

    public GameObject addArrow(GameObject arrow, Transform start, Transform target, float rangeForce, float damage)
    {
        GameObject tempArrow;
        if (dieListBullet.Count > 0)
        {
            tempArrow = dieListBullet.Last.Value;
            tempArrow.SetActive(true);
            tempArrow.GetComponent<bulletSys>().shotSetting(start.transform.position, target.transform.position, rangeForce, damage, this);
            dieListBullet.RemoveLast();
            aliveListBullet.AddLast(tempArrow);
            return tempArrow;
        }

        tempArrow = Instantiate(arrow, arrow.transform.position, arrow.transform.rotation, transform);
        tempArrow.GetComponent<bulletSys>().shotSetting(start.transform.position, target.transform.position, rangeForce, damage, this);
        aliveListBullet.AddLast(tempArrow);

        return aliveListBullet.FindLast(tempArrow).Value;
    }

    public void addDieList(GameObject dieGo)
    {
        aliveListBullet.Remove(dieGo);
        dieListBullet.AddLast(dieGo);
    }
}
