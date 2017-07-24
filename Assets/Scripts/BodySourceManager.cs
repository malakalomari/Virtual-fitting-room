using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;

public class BodySourceManager : MonoBehaviour 
{
	/******************************************************
     * PRIVATE
     *****************************************************/
    private KinectSensor sensor;
    private BodyFrameReader reader;
    private Body[] data = null;
    private Dictionary<ulong, Body> bodies = new Dictionary<ulong, Body>();
    private List<ulong> bodyOrder = new List<ulong>();

    /******************************************************
     * START
     *****************************************************/
    void Start () 
    {
        this.sensor = KinectSensor.GetDefault();

        if (this.sensor != null)
        {
            this.reader = this.sensor.BodyFrameSource.OpenReader();
            
            int bodycount = this.sensor.BodyFrameSource.BodyCount;

            this.data = new Body[bodycount];

            if (!this.sensor.IsOpen)
            {
                this.sensor.Open();
            }
        }   
    }

    /******************************************************
     * UPDATE
     *****************************************************/
    void Update () 
    {
        if (this.reader != null)
        {
            var frame = this.reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (this.data == null)
                {
                    this.data = new Body[this.sensor.BodyFrameSource.BodyCount];
                }
                
                frame.GetAndRefreshBodyData(this.data);              
                frame.Dispose();
                frame = null;
            }

            if (this.data == null) { return; }

            List<ulong> trackedIds = new List<ulong>();
            foreach (var body in this.data)
            {
                if (body != null)
				{
	                if (body.IsTracked)
	                {
	                    trackedIds.Add(body.TrackingId);
	                }
				}
            }

            List<ulong> knownIds = new List<ulong>(bodies.Keys);
            foreach (ulong trackingId in knownIds)
            {
                if (!trackedIds.Contains(trackingId))
                {
                    this.bodies.Remove(trackingId);
                    this.bodyOrder.Remove(trackingId);
                }
            }

            foreach (var body in this.data)
            {
                if (body != null)
				{
	                if (body.IsTracked)
	                {
	                    if (!this.bodies.ContainsKey(body.TrackingId))
	                    {
	                        this.bodyOrder.Add(body.TrackingId);
	                        this.bodies[body.TrackingId] = body;
	                    }
	                }
				}
            }
        }
    }

    /******************************************************
     * GETTERS
     *****************************************************/
    public Body[] GetData()
    {
        return this.data;
    }

    public Dictionary<ulong, Body> GetBodies()
    {
        return this.bodies;
    }

    public int OrderOf(ulong id)
    {
        return this.bodyOrder.IndexOf(id);
    }

    /******************************************************
     * EVENTS
     *****************************************************/
    void OnApplicationQuit()
    {
        if (this.reader != null)
        {
            this.reader.Dispose();
            this.reader = null;
        }
        
        if (this.sensor != null)
        {
            if (this.sensor.IsOpen)
            {
                this.sensor.Close();
            }     
            this.sensor = null;
        }
    }
}
