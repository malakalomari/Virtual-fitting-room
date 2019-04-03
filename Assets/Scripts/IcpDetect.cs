using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Kinect;
using Kinect = Windows.Kinect;


// Define the functions which can be called from the .dll.
internal static class OpenCVInterop
{
	[DllImport("Project1")]
	internal unsafe static extern void icpGetCurrentICPModel(double[,] ModelPointsArr, double[,] DataPointsArr, double[,] transformedPointsArr);
}

public class IcpDetect: MonoBehaviour {
	public GameObject BodySrcManager;
	
	public JointType TrackedJoint;
	public JointType TrackedJoint2;
	public JointType TrackedJoint3;

	public GameObject leftHip;
	public GameObject rightHip;
	public GameObject leftKnee;


	public JointType HandRight = JointType.HandRight;
	public JointType HandLeft = JointType.HandLeft;
	public JointType ElbowRight= JointType.ElbowRight;
	public JointType ElbowLeft = JointType.ElbowLeft;
	public JointType ShoulderLeft = JointType.ShoulderLeft;
	public JointType ShoulderRight = JointType.ShoulderRight;
	int x = 0;

	public BodySourceManager bodyManager;
	private Body[] bodies;
	private float scaleX;
	private float scaleY;
	public Vector3 multiplier;
	//public Vector3 scaleMultiplier;

    void Start() {
	}
	
	// Update is called once per frame
	void Update () {
		if (bodyManager == null)
		{
			return;
		}
		bodies = bodyManager.GetData();
		if (bodies == null)
		{
			return;
		}
		foreach (var body in bodies)
		{
			if(body == null)
			{
				continue;
			}
				// get a valid and tracked body
			if (body.IsTracked)
			{
				//check for t-pose
				var rh = body.Joints[HandRight].Position;
				var lh = body.Joints[HandLeft].Position;
				var rsh = body.Joints[ElbowRight].Position;
				var lsh = body.Joints[ElbowLeft].Position;
				var rel = body.Joints[ShoulderLeft].Position;
				var lel = body.Joints[ShoulderRight].Position;

				if (
					Mathf.Abs(rel.Y - rsh.Y)< 0.1f && 
					Mathf.Abs(rh.Y - rsh.Y) < 0.1f &&
					Mathf.Abs(lel.Y - lsh.Y) < 0.1f && 
					Mathf.Abs(lh.Y - lsh.Y) < 0.1f
					)
				{
					x = 1;
								
					scaleX = transform.localScale.x;
					scaleY = transform.localScale.y;
					GameObject tPoseText = GameObject.Find("T-Pose");
					if (tPoseText) {
						tPoseText.SetActive(false);
					}
				}
				if (x == 1)
				{
					// pass to the filter object
					var jointPosition1 = body.Joints[TrackedJoint].Position;
					var jointPosition2 = body.Joints[TrackedJoint2].Position;
					var jointPosition3 = body.Joints[TrackedJoint3].Position;

					double[,] ModelPointsArr = new double[,] {
						{ jointPosition1.X, jointPosition2.X, jointPosition3.X },
						{ jointPosition1.Y, jointPosition2.Y, jointPosition3.Y },
						{ jointPosition1.Z, jointPosition2.Z, jointPosition3.Z }
					};

					// print("model points array");
					// print(jointPosition1.X + " " + jointPosition2.X + " " + jointPosition3.X );
					// print(jointPosition1.Y + " " + jointPosition2.Y + " " + jointPosition3.Y );
					// print(jointPosition1.Z + " " + jointPosition2.Z + " " + jointPosition3.Z );


					var pivotPointPosition1 = leftHip.transform.position;
					var pivotPointPosition2 = rightHip.transform.position;
					var pivotPointPosition3 = leftKnee.transform.position;

					double[,] DataPointsArr = new double[,] {
						{ pivotPointPosition1.x, pivotPointPosition2.x, pivotPointPosition3.x },
						{ pivotPointPosition1.y, pivotPointPosition2.y, pivotPointPosition3.y },
						{ pivotPointPosition1.z, pivotPointPosition2.z, pivotPointPosition3.z }
					};

					// print("data points array");
					// print(pivotPointPosition1.x + " " + pivotPointPosition2.x + " " + pivotPointPosition3.x );
					// print(pivotPointPosition1.y + " " + pivotPointPosition2.y + " " + pivotPointPosition3.y );
					// print(pivotPointPosition1.z + " " + pivotPointPosition2.z + " " + pivotPointPosition3.z );

					double[,] transformedPointsArr = new double[3, 3];

					OpenCVInterop.icpGetCurrentICPModel(ModelPointsArr, DataPointsArr, transformedPointsArr);


					// print("transform point");
					// print(transformedPointsArr[0, 0]);
					// print(transformedPointsArr[1, 0]);
					// print(transformedPointsArr[2, 0]);
					var px1 = (float)transformedPointsArr[0, 0];
					var py1 = (float)transformedPointsArr[1, 0];
					var pz1 = (float)transformedPointsArr[2, 0];
					var v1 = new Vector3(px1 * multiplier.x, py1 * multiplier.y, pz1 * multiplier.z);
					transform.position = v1;

					Quaternion actualRotation = GetQuaternion(body.JointOrientations[TrackedJoint]);
					transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(actualRotation.x, actualRotation.y * 0.5f, actualRotation.z, actualRotation.w), Time.deltaTime * 25);
					float x = actualRotation.x;
					float y = actualRotation.y;
					float z = actualRotation.z;
					float w = actualRotation.w;

					// convert  rotation quaternion to Euler angles in degrees
					double yawD, pitchD, rollD;
					pitchD = Mathf.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Mathf.PI * 180.0;
					yawD = Mathf.Asin(2 * ((w * y) - (x * z))) / Mathf.PI * 180.0;
					rollD = Mathf.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Mathf.PI * 180.0;
					// transform.position = Vector3.Lerp(transform.position, new Vector3(pos.X * multiplier.x, pos.Y * multiplier.y, pos.Z * multiplier.z), Time.deltaTime * 25);
					//var height = Math.Abs(body.Joints[JointType.SpineMid].Position.Y - body.Joints[JointType.KneeLeft].Position.Y);
					//var width = Math.Abs(body.Joints[JointType.ShoulderLeft].Position.X - body.Joints[JointType.ShoulderRight].Position.X);
					//transform.localScale = new Vector3(width * scaleMultiplier.x, height * scaleMultiplier.y, width * scaleMultiplier.z);

                }
			}
		}
	}
	public static Quaternion GetQuaternion(Kinect.JointOrientation jointOrientation)
	{
		return GetQuaternion(jointOrientation.Orientation);
	}
	public static Quaternion GetQuaternion(Kinect.Vector4 vector)
	{
		return new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
	}
}