using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCraftingUI : MonoBehaviour {
    public delegate void buildingUIHandler(bool isOn);
    public static event buildingUIHandler OnBuildingUI;

    public delegate void playerUIHandler(bool isOn);
    public static event playerUIHandler OnPlayerUI;

    public delegate void monsterUIHandler(bool isOn);
    public static event monsterUIHandler OnMonsterUI;

    public delegate void offAllUI();
    public static event offAllUI offallUI;

    public bool isBuildingListSelect;
    private GameObject BuildingButton;
    private GameObject PlayerButton;
    private GameObject EnemyButton;

    private void Awake()
    {
        BuildingButton = GameObject.Find("BuildingCraftingButton");
        PlayerButton = GameObject.Find("PlayerModeButton");
        EnemyButton = GameObject.Find("EnemyModeButton");
    }

    private void LateUpdate()
    {
        if (BuildingButton.activeSelf && PlayerButton.activeSelf && EnemyButton.activeSelf) AllOffUI();
    }

    public void UIon(GameObject ui)
    {
        if (!ui.activeSelf)
        {
            ui.SetActive(true);
        }
        else
        {
            ui.SetActive(false);

            // 자식 버튼 취소 처리
            isBuildingListSelect = false;
        }
    }

    public void isUI(GameObject ui) { ui.SetActive(!ui.activeSelf); }

    public void OnbuildingUI()
    {
        if (OnPlayerUI != null)     OnPlayerUI(false);
        if (OnMonsterUI != null)    OnMonsterUI(false);
        if (OnBuildingUI != null)   OnBuildingUI(true);
    }

    public void OnplayerUI()
    {
        if (OnMonsterUI != null)     OnMonsterUI(false);
        if (OnBuildingUI != null)    OnBuildingUI(false);
        if (OnPlayerUI != null)   OnPlayerUI(true);
    }

    public void OnmonsyerUI() {
        if (OnPlayerUI != null)     OnPlayerUI(false);
        if (OnBuildingUI != null)    OnBuildingUI(false);
        if (OnMonsterUI != null)   OnMonsterUI(true);
    }

    public void AllOffUI()
    {
       if (OnPlayerUI != null)      OnPlayerUI(false);
       if (OnBuildingUI != null)     OnBuildingUI(false);
       if (OnMonsterUI != null)    OnMonsterUI(false);
    }

}