using System.Collections;
using System.Collections.Generic;
using hakoniwa.pdu.interfaces;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.pdu.msgs.hako_msgs;
using hakoniwa.sim;
using hakoniwa.sim.core;
using UnityEngine;

public class Robot : MonoBehaviour, IHakoObject
{
    IHakoPdu hakoPdu;
    string robotName = "Baggage1";
    string pduName = "pos";
    string droneName = "DroneTransporter";
    string pduDisturbance = "drone_disturbance";

    public void EventInitialize()
    {
        hakoPdu = HakoAsset.GetHakoPdu();
        hakoPdu.DeclarePduForRead(robotName, pduName);
        hakoPdu.DeclarePduForWrite(droneName, pduDisturbance);
    }

    public void EventTick()
    {
        var pduManager = hakoPdu.GetPduManager();

        /*
         * For Read
         */
        IPdu pdu = pduManager.ReadPdu(robotName, pduName);
        Twist pos = new Twist(pdu);

        Debug.Log($"Tick Event Occured: {robotName}({pos.linear.x}, {pos.linear.y}, {pos.linear.z}");


        /*
         * For write
         */
        IPdu pdu_dis = pduManager.CreatePdu(droneName, pduDisturbance);
        Disturbance dis = new Disturbance(pdu_dis);

        dis.d_temp.value = 100;
        pduManager.WritePdu(droneName, pdu_dis);
        pduManager.FlushPdu(droneName, pduDisturbance);
    }
    public void EventReset()
    {
        Debug.Log("Reset Event Occured");
    }

    public void EventStart()
    {
        Debug.Log("Start Event Occured");
    }

    public void EventStop()
    {
        Debug.Log("Stop Event Occured");
    }
}
