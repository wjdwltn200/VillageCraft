using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapMgr : MonoBehaviour {
    public int tileSizeXY;
    public GameObject tileColl;

    private Transform tileGround;
    private TileMapSetting tileFloor;

    private bool isMap = true;

    private void Awake()
    {
        // 타일 크기에 맞게 바닥 타일 지정
        tileGround = GameObject.Find("TileGround").GetComponent<Transform>();
        tileGround.localScale = new Vector3(tileSizeXY / 2 * 10, 0.1f, tileSizeXY / 2 * 10);

        // 자식 스크립트 정보 받아오기
        tileFloor = GameObject.Find("TileFloor").GetComponent<TileMapSetting>();
        tileFloor.tileSizeXY = tileSizeXY;
        tileFloor.tileGo = tileColl;
    }

    private void Update()
    {
        if (isMap)
        {
            tileFloor.tileCreate();
            tileColl = null;
            isMap = false;
        }
    }
}
