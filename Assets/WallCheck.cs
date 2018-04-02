using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour {

    private PlayerController pc;

	// Use this for initialization
	void Start () {
        pc = GetComponentInParent<PlayerController>();
	}
}
