using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCameraAroundOrigin : MonoBehaviour {

	private Vector3 lastMousePosition;

	void Update () {
		if (!Input.GetMouseButton (0)) return;

		if (Input.GetMouseButtonDown (0)) {
			lastMousePosition = Input.mousePosition;
		}

		var delta = (Input.mousePosition - lastMousePosition) * Time.deltaTime;

		var go = this.gameObject;
		go.transform.RotateAround (Vector3.zero, Vector3.up, 10 * delta.x);
		go.transform.RotateAround (Vector3.zero, go.transform.right, -10 * delta.y);

		lastMousePosition = Input.mousePosition;
	}
}
