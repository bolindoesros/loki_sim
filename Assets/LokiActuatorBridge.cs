using MDS.FlightController;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

/// <summary>
/// Subscribes to a ROS2 PWM topic and drives either a thruster or a fin servo.
/// Assign one component per actuator in the Inspector.
///
/// Topic assignments:
///   Thruster port + stbd → /cmd/thruster
///   Elevator port + stbd → /cmd/elevator
///   Rudder top + bottom  → /cmd/rudder
/// </summary>
public class LokiActuatorBridge : MonoBehaviour
{
    [Header("ROS Settings")]
    public string pwmTopic = "/cmd/elevator";

    [Header("Actuator Type")]
    public bool isThruster = false;
    public Propeller targetPropeller;   // assign for thrusters only

    [Header("Fin Settings")]
    public float maxAngleDeg = 30f;
    public Vector3 rotationAxis = new Vector3(1, 0, 0);

    [Header("Safety")]
    public float timeoutSeconds = 0.5f;

    private ROSConnection ros;
    private Quaternion initialRotation;
    private int currentPwm = 1500;
    private float lastMessageTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Int32Msg>(pwmTopic, OnPwm);
        initialRotation = transform.localRotation;
        lastMessageTime = Time.time;
    }

    void OnPwm(Int32Msg msg)
    {
        currentPwm = Mathf.Clamp(msg.data, 1100, 1900);
        lastMessageTime = Time.time;
    }

    void FixedUpdate()
    {
        // Safety timeout — revert to neutral if no message received
        if (Time.time - lastMessageTime > timeoutSeconds)
            currentPwm = 1500;

        if (isThruster && targetPropeller != null)
        {
            targetPropeller.ApplyPwm(currentPwm);
        }
        else
        {
            // Map PWM 1100-1900 → angle -maxAngle to +maxAngle
            float targetAngle = (currentPwm - 1500) * (maxAngleDeg / 400f);
            transform.localRotation = initialRotation *
                Quaternion.AngleAxis(targetAngle, rotationAxis);
        }
    }
}