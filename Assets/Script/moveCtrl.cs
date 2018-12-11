using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCtrl : MonoBehaviour {
    public float moveSpeed;
    public float rotSpeed;
    public float h;
    public float v;

    public bool isAtk;

    public Space tSpace;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        h = Input.GetAxis("MoveInput");
        v = Input.GetAxis("Vertical");
        moveInputKey();
    }

    void moveInputKey()
    {
        transform.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime, tSpace);
        transform.Rotate(0, h * rotSpeed * Time.deltaTime, 0, tSpace);
    }
}
