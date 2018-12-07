using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCtrl : MonoBehaviour {
    public float moveSpeed;
    public float rotSpeed;
    public float h;
    public float v;
    public bool isAtk = false;

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
        if (Input.GetKeyDown(KeyCode.Space)) isAtk = true;

        transform.Translate(Vector3.forward * v * moveSpeed * Time.deltaTime);
        transform.Rotate(0, h * rotSpeed * Time.deltaTime, 0);
    }
}
