using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;

public class SmoothJoint 
{
	public JointType jointType;
	public int queueSize = 15;	
	private Queue<Quaternion> rotations;
	private Quaternion rotationMean;	
	private Queue<Vector3> positions;
	private Vector3 positionMean;
	
	public SmoothJoint(JointType jointType) 
	{
		this.jointType = jointType;
		this.rotationMean = Quaternion.Euler(0, 0, 0);
		this.positionMean = Vector3.zero;
		this.rotations = new Queue<Quaternion>();
		this.positions = new Queue<Vector3>();
		for (int i = 1; i < this.queueSize; i += 1)
		{
			this.rotations.Enqueue(this.rotationMean);
			this.positions.Enqueue(this.positionMean);
		}
	}
	
	public Vector3 GetPosition()
	{
		return this.positionMean;
	}
	
	public Quaternion GetRotation()
	{
		return this.rotationMean;
	}
		
	public void Update(Body body)
	{
		if (!Utils.IsEndJoint(this.jointType))
		{
			Quaternion actualRotation = Utils.GetQuaternion(body.JointOrientations[this.jointType]);	
			this.rotations.Enqueue(actualRotation);
			this.rotationMean = RotationSmoothFilter(this.rotations, this.rotationMean);
			this.rotations.Dequeue();
		}				
		
		Vector3 actualPosition = Utils.GetPosition(body.Joints[this.jointType]);
		this.positions.Enqueue(actualPosition);
		this.positionMean = PositionSmoothFilter(this.positions, this.positionMean);
		this.positions.Dequeue(); 
	}
	
	private Vector3 PositionSmoothFilter(Queue<Vector3> positions, Vector3 lastMedian)
	{
		Vector3 median = Vector3.zero;
		
		foreach(Vector3 position in positions)
		{
			median += position;
		}
		
		median /= positions.Count;
		
		return median;
	}
	
	private Quaternion RotationSmoothFilter(Queue<Quaternion> quaternions, Quaternion lastMedian)
	{
		Quaternion median = new Quaternion(0, 0, 0, 0);
		foreach (Quaternion quaternion in quaternions)
		{
			float weight = 1 - (Quaternion.Dot(lastMedian, quaternion) / (Mathf.PI / 2)); // 0 degrees of difference => weight 1. 180 degrees of difference => weight 0.
			Quaternion weightedQuaternion = Quaternion.Lerp(lastMedian, quaternion, weight);
			
			median.x += weightedQuaternion.x;
			median.y += weightedQuaternion.y;
			median.z += weightedQuaternion.z;
			median.w += weightedQuaternion.w;
		}
		
		median.x /= quaternions.Count;
		median.y /= quaternions.Count;
		median.z /= quaternions.Count;
		median.w /= quaternions.Count;
		
		return NormalizeQuaternion(median);
	}
	
	private Quaternion NormalizeQuaternion(Quaternion quaternion)
	{
		float x = quaternion.x, y = quaternion.y, z = quaternion.z, w = quaternion.w;
		float length = 1.0f / (w * w + x * x + y * y + z * z);
		return new Quaternion(x * length, y * length, z * length, w * length);
	}
}
