using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveKey : MonoBehaviour {
    private float h = 0.0f;
    private float v = 0.0f;
    public float moveSpeed;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        h = Input.GetAxis("MoveInput");
        v = Input.GetAxis("Vertical");
        movement();
    }

    public void movement()
    {
        transform.Translate(moveSpeed * h * Time.deltaTime, 0, moveSpeed * v * Time.deltaTime, Space.World);

        if (Input.GetKey(KeyCode.Q))
            transform.Translate(0, 0, moveSpeed * Time.deltaTime, Space.Self);
        if (Input.GetKey(KeyCode.E))
            transform.Translate(0, 0, -moveSpeed * Time.deltaTime, Space.Self);
    }
}
