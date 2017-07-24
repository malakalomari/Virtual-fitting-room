using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class Utils 
{
    public static Dictionary<Kinect.JointType, Kinect.JointType> BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },     
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },     
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },   
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
		{ Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
    public static bool IsEndJoint(Kinect.JointType jointType)
    {
    	return jointType == Kinect.JointType.Head ||
				jointType == Kinect.JointType.FootLeft ||
				jointType == Kinect.JointType.FootRight ||
				jointType == Kinect.JointType.HandTipLeft ||
				jointType == Kinect.JointType.HandTipRight ||
				jointType == Kinect.JointType.ThumbLeft ||
				jointType == Kinect.JointType.ThumbRight;    	
    }
    
	public static Vector3 GetPosition(Kinect.CameraSpacePoint point)
	{
		return new Vector3(point.X, point.Y, point.Z);
	}

    public static Vector3 GetPosition(Kinect.Joint joint)
    {
        return GetPosition(joint.Position);
    }

    public static Quaternion GetQuaternion(Kinect.Vector4 vector)
    {    	
        return new Quaternion(vector.X, vector.Y, vector.Z, vector.W);        
    }

    public static Quaternion GetQuaternion(Kinect.JointOrientation jointOrientation)
    {
        return GetQuaternion(jointOrientation.Orientation);
    }
}
