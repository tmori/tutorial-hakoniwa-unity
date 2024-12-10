using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using hakoniwa.sim;
using hakoniwa.sim.core;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


namespace hakoniwa.gui
{
    public class SimStart : MonoBehaviour
    {
        Text my_text;
        Button my_btn;
        private enum SimCommandStatus
        {
            WaitStart = 0,
            WaitStop = 1,
            WaitReset = 2,
        }
        private SimCommandStatus cmd_status = SimCommandStatus.WaitStart;
        bool isResetHappened = false;

        void Start()
        {
            var obj = GameObject.Find("StartButton");
            my_btn = obj.GetComponentInChildren<Button>();
            my_text = obj.GetComponentInChildren<Text>();
            my_btn.interactable = true;
        }
        void Update()
        {
            HakoSimState state = HakoAsset.GetHakoControl().GetState();
            if ((state != HakoSimState.Running) && (state != HakoSimState.Stopped))
            {
                my_btn.interactable = false;
                //Debug.Log("Disabled interactable button because of state: " + state);
                return;
            }
            else
            {
                my_btn.interactable = true;
                //Debug.Log($"Enabled interactable button because of state({state} cmd_status({cmd_status})");
            }
            {

            }
            //cmd status changer
            switch (cmd_status)
            {
                case SimCommandStatus.WaitStart:
                    if (state == HakoSimState.Running)
                    {
                        my_text.text = "STOP";
                        cmd_status = SimCommandStatus.WaitStop;
                    }
                    break;
                case SimCommandStatus.WaitStop:
                    if (state == HakoSimState.Stopped)
                    {
                        my_text.text = "RESET";
                        cmd_status = SimCommandStatus.WaitReset;
                    }
                    break;
                case SimCommandStatus.WaitReset:
                    if (isResetHappened && state == HakoSimState.Stopped)
                    {
                        my_text.text = "START";
                        cmd_status = SimCommandStatus.WaitStart;
                    }
                    break;
                default:
                    break;
            }
        }
        public void OnButtonClick()
        {
            Debug.Log("button is clicked");
            IHakoControl simulator = HakoAsset.GetHakoControl();
            switch (cmd_status)
            {
                case SimCommandStatus.WaitStart:
                    simulator.SimulationStart();
                    my_btn.interactable = false;
                    break;
                case SimCommandStatus.WaitStop:
                    simulator.SimulationStop();
                    my_btn.interactable = false; 
                    break;
                case SimCommandStatus.WaitReset:
                    simulator.SimulationReset();
                    isResetHappened = true;
                    my_btn.interactable = false;
                    break;
                default:
                    break;
            }
        }
    }
}

