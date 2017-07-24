using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShoulderScaleBased : MonoBehaviour {

	public Vector3 left;
	public Vector3 right;

	public Transform leftShoulderJoint;
	public Transform rightShoulderJoint;

	public float multiplier = 20;

	void Update ()
	{
		float offset = 
			Vector3.Distance(
				left, 
				right)
			/ 
			Vector3.Distance(
				leftShoulderJoint.position, 
				rightShoulderJoint.position
			);
		transform.localScale = Vector3.one * (offset > 0.00001f ? Vector3.Distance(leftShoulderJoint.position, rightShoulderJoint.position) / Vector3.Distance(left, right) : 1) * multiplier;
	}

	void OnDrawGizmos ()
	{
		Gizmos.DrawLine(transform.position + left, transform.position + right);
	}
}