using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;

public class BodySmoothController : MonoBehaviour 
{
	public BodySourceManager bodyManager;
	private Dictionary<JointType, SmoothJoint> smoothJoints;
	private 
	
	// Use this for initialization
	void Start () 
	{
		this.smoothJoints = new Dictionary<JointType, SmoothJoint>();
		for (JointType jt = JointType.SpineBase; jt <= JointType.ThumbRight; jt++)
		{
			this.smoothJoints[jt] = new SmoothJoint(jt);
		}
		
		foreach (KeyValuePair<JointType, JointType> pair in Utils.BoneMap)
		{
			
			
			CreateBodyElement(pair.Key.ToString() + "-" + pair.Value.ToString(), 0.1f);
			
		}
	}
	
	void Update ()
	{
		foreach (KeyValuePair<ulong, Body> pairIdBody in this.bodyManager.GetBodies())
		{
			if (this.bodyManager.OrderOf(pairIdBody.Key) == 0)
			{
				RefreshBody(pairIdBody.Value);
			}
		
		}
	}
	
	private void RefreshBody(Body body)
	{
		foreach (KeyValuePair<JointType, SmoothJoint> pairJointTransform in this.smoothJoints)
		{
			pairJointTransform.Value.Update(body);
		}
		
		foreach (KeyValuePair<JointType, JointType> pair in Utils.BoneMap)
		{
			Transform jointObj = this.transform.Find(pair.Key.ToString() + "-" + pair.Value.ToString());
			
			jointObj.localRotation = this.smoothJoints[pair.Key].GetRotation();
			jointObj.localPosition = Vector3.Lerp(this.smoothJoints[pair.Key].GetPosition(), this.smoothJoints[pair.Value].GetPosition(), 0.5f);
			jointObj.localScale = new Vector3(0.05f, Vector3.Distance(this.smoothJoints[pair.Key].GetPosition(), this.smoothJoints[pair.Value].GetPosition()), 0.05f);
		}
	}
	
	private void CreateBodyElement(string name, float scale)
	{
		Transform cube = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
		cube.gameObject.layer = 0;
		cube.name = name;
		cube.transform.parent = this.transform;
		cube.transform.localScale = Vector3.one * scale;
	}	
}
