using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class ShiftMass : MonoBehaviour
{
    private ArticulationBody _MovingMassBody;
    public string topicName = "/cmd/moving_mass";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _MovingMassBody = GetComponent<ArticulationBody>();
        ROSConnection.GetOrCreateInstance().Subscribe<Float64Msg>(topicName, OnReceiveMassPosition);
    }


    void OnReceiveMassPosition(Float64Msg msg)
    {
        var drive = _MovingMassBody.zDrive;
        drive.target = (float)msg.data;
        _MovingMassBody.zDrive = drive;
    }
}

