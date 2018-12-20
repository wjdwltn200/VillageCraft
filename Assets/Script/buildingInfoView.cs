using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class buildingInfoView : MonoBehaviour {
    public BuildingData buildingData;

    public Text _name;
    public Text _HP;
    public Text _ATK;
    public Text _AtkDealy;
    public Text _Range;
    public Text _RangeRadius;
    public Text _TaxPoint;
    public Text _BuyPrice;

    public void setBuilding(BuildingData setBuildingData) {
        buildingData = setBuildingData;
        setting();
    }

    private void setting()
    {
        _name.text = buildingData._name;
        _HP.text = "체력 : " + buildingData._maxHP.ToString() + " / " + buildingData._maxHP.ToString();
        _ATK.text = "공격력 : " + buildingData._atkPoint.ToString();
        _AtkDealy.text = "공격 속도 : " + buildingData._atkDelay.ToString();
        _Range.text = "사거리 : " + buildingData._atkRange.ToString();
        _RangeRadius.text = "조준력 : " + buildingData._rangeRadius.ToString();
        _TaxPoint.text = "수입 : " + buildingData._taxPoint.ToString();
        _BuyPrice.text = "건설 가격 : " + buildingData._buyPrice.ToString();
    }
}
