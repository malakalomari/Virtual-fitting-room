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

public class DetectJoints : MonoBehaviour {
    public GameObject BodySrcManager;
    public JointType TrackedJoint;
    public JointType TrackedJoint2;
    public JointType TrackedJoint3;

    public JointType HandRight = JointType.HandRight;
    public JointType HandLeft = JointType.HandLeft;
    public JointType ElbowRight= JointType.ElbowRight;
    public JointType ElbowLeft = JointType.ElbowLeft;
    public JointType ShoulderLeft = JointType.ShoulderLeft;
    public JointType ShoulderRight = JointType.ShoulderRight;
    int x = 0;

	public BodySourceManager bodyManager;
    private Body[] bodies;
	public Vector3 multiplier;

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
        { return;
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

                if (Mathf.Abs(rel.Y - rsh.Y)< 0.1f && 
                    Mathf.Abs(rh.Y - rsh.Y) < 0.1f &&
                    Mathf.Abs(lel.Y - lsh.Y) < 0.1f && 
                    Mathf.Abs(lh.Y - lsh.Y) < 0.1f)
                {
                    x = 1;

					GameObject tPoseText = GameObject.Find("T-Pose");

					if (tPoseText)
						tPoseText.SetActive(false);
                }

                if (x == 1)
                {
                    // pass to the filter object
					var pos = body.Joints[TrackedJoint].Position;
                    var pos2 = body.Joints[TrackedJoint2].Position;
                    var pos3 = body.Joints[TrackedJoint3].Position;

                    double[,] ModelPointsArr = new double[,] { { pos.X, pos.Y, pos.Z }, { pos2.X, pos2.Y, pos2.Z }, { pos3.X, pos3.Y, pos3.Z } };

                    // point 1 => knee , point2 => hip right, point3 => hip left
                    var width = pos2.X - pos.X;
                    var hight = pos.Y - pos3.Y;

                    double[,] DataPointsArr = new double[,] {
                        { transform.position.x + (width / 2), transform.position.y, transform.position.z },
                        { transform.position.x + (width / 2), transform.position.y + hight, transform.position.z },
                        { transform.position.x - (width / 2), transform.position.y + hight, transform.position.z }
                     };

                    double[,] transformedPointsArr = new double[3, 3];

                    OpenCVInterop.icpGetCurrentICPModel(ModelPointsArr, DataPointsArr, transformedPointsArr);

                    var px1 = (float)transformedPointsArr[0, 0];
                    var py1 = (float)transformedPointsArr[0, 1];
                    var pz1 = (float)transformedPointsArr[0, 2];
                    var v1 = new Vector3(px1, py1, pz1);

                    // transform.position = v1;
                    Quaternion actualRotation = GetQuaternion(body.JointOrientations[TrackedJoint]);
					float x = actualRotation.x;
					float y = actualRotation.y;
					float z = actualRotation.z;
					float w = actualRotation.w;

					// convert  rotation quaternion to Euler angles in degrees
					double yawD, pitchD, rollD;
					pitchD = Mathf.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Mathf.PI * 180.0;
					yawD = Mathf.Asin(2 * ((w * y) - (x * z))) / Mathf.PI * 180.0;
					rollD = Mathf.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Mathf.PI * 180.0;

					transform.position = Vector3.Lerp(transform.position, new Vector3(pos.X * multiplier.x, pos.Y * multiplier.y, pos.Z * multiplier.z), Time.deltaTime * 25);
					transform.rotation = Quaternion.Lerp(transform.rotation, new Quaternion(actualRotation.x, actualRotation.y * 0.5f, actualRotation.z, actualRotation.w), Time.deltaTime * 25);
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