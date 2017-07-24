
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using Kinect = Windows.Kinect;



public class DetectJoints : MonoBehaviour {
    public GameObject BodySrcManager;
    public JointType TrackedJoint;
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
    //Quaternion jointRotation;
//    KinectJointOrientationFilter filter = new KinectJointOrientationFilter();

    // change params if you want
    void Start() {

//        filter.Init(); // change params if you want
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
              
//                filter.UpdateFilter(body);
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
//                    Windows.Kinect.Vector4 c = filter.GetFilteredRotationJoint((int)TrackedJoint);
                    // Quaternion v = GetQuaternion(c);
					var pos = body.Joints[TrackedJoint].Position;
                    //    var rot = body.JointOrientations[TrackedJoint].Orientation;
                    // jointRotation.Set(rot.X, rot.Y, rot.Z, rot.W);
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

					print(pitchD);
					print(yawD);
					print(rollD);

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