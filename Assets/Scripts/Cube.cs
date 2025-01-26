using System;
using System.Collections;
using System.Collections.Generic;
using hakoniwa.pdu.msgs.geometry_msgs;
using hakoniwa.sim;
using hakoniwa.sim.core;
using UnityEngine;

public class Cube : MonoBehaviour, IHakoObject
{
    public string robotName = "Cube";
    public string pduName = "pos";
    private IHakoPdu hakoPdu;
    public void EventInitialize()
    {
        hakoPdu = HakoAsset.GetHakoPdu();
        var ret = hakoPdu.DeclarePduForRead(robotName, pduName);
        if (ret == false)
        {
            throw new ArgumentException($"Can not declare pdu for read: {robotName} {pduName}");
        }
    }

    public void EventReset()
    {
    }

    public void EventStart()
    {
    }

    public void EventStop()
    {
    }

    public void EventTick()
    {
        var pduManager = hakoPdu.GetPduManager();
        var pdu = pduManager.ReadPdu(robotName, pduName);
        if (pdu == null)
        {
            Debug.Log($"Can not find pdu:{robotName}/{pduName};");
            return;
        }
        Twist twist = new Twist(pdu);
        this.transform.position = hakoniwa.pdu.Frame.toUnityPosFromPdu(twist.linear);
    }
}
