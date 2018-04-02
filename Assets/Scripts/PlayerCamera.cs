using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    public float turnSpeed = 5;

    private Vector3 mousePosition;

	// Use this for initialization
	void Start () {
        Application.targetFrameRate = 144;
	}
	
	// Update is called once per frame
	void Update () {
        float mouseXDelta = Input.GetAxis("Mouse X");
        float mouseYDelta = Input.GetAxis("Mouse Y");
        transform.Rotate(Vector3.up * mouseXDelta * turnSpeed, Space.World);
        transform.Rotate(transform.right * -mouseYDelta * turnSpeed, Space.World);
	}
}
