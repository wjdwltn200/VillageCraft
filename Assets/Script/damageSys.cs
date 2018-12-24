using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum currType
{
    hero,
    enemy,
    building
}

public class damageSys : MonoBehaviour {

    private GameObject currData;
    public currType currType;

    private void Awake()
    {
        currData = gameObject;
    }

    public void setHpPoint (float dam)
    {
        switch (currType)
        {
            case currType.hero:
                currData.GetComponent<HeroAI>()._currHP -= dam;
                break;
            case currType.enemy:
                currData.GetComponent<MonsAI>()._currHP -= dam;
                break;
            case currType.building:
                currData.GetComponent<BuildingData>()._currHP -= dam;
                StartCoroutine(currData.GetComponent<BuildingData>().SetShaking());
                break;
            default:
                Debug.Log("데미지 시스템 에러");
                break;
        }
    }

    public float getHpPoint ()
    {
        switch (currType)
        {
            case currType.hero:
                return currData.GetComponent<HeroAI>()._currHP;
            case currType.enemy:
                return currData.GetComponent<MonsAI>()._currHP;
            case currType.building:
                return currData.GetComponent<BuildingData>()._currHP;
            default:
                Debug.Log("데미지 시스템 에러");
                break;
        }
        return 0.0f;
    }

}
