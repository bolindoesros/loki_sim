using UnityEngine;
using UnitySensors.Sensor.TF;

[System.Serializable]
public struct RandomGenerator
{
    public float min;
    public float max;

    public RandomGenerator(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public readonly float RandomValue()
    {
        return Random.Range(min, max);
    }
}

public class RandomTeleporter : MonoBehaviour
{
    [Tooltip("The object to teleport around. Only handle Articulation Body.")]
    [SerializeField] string vehicleName;
    [Tooltip("Expected location to teleport in.")]
    [SerializeField] Transform expectedLocation;

    [SerializeField] RandomGenerator horizontalRange;
    [SerializeField] RandomGenerator forwardRange;
    [SerializeField] RandomGenerator yawRange;

    // Start is called before the first frame update
    void Start()
    {
        GameObject vehicleObj = GameObject.Find(vehicleName);
        Transform transformToTeleport = vehicleObj.transform;
        
        var unityPosi = new Vector3(
                        expectedLocation.position.x + horizontalRange.RandomValue(),
                        expectedLocation.position.y,
                        expectedLocation.position.z + forwardRange.RandomValue());
        var unityOri = Quaternion.Euler(
                        expectedLocation.rotation.eulerAngles.x,
                        expectedLocation.rotation.eulerAngles.y + yawRange.RandomValue(),
                        expectedLocation.rotation.eulerAngles.z);

        transformToTeleport.SetPositionAndRotation(unityPosi, unityOri);

        if (transformToTeleport.TryGetComponent<ArticulationBody>(out var artBody))
        {
            artBody.TeleportRoot(unityPosi, unityOri);
        }

        if (transformToTeleport.TryGetComponent<TFLink>(out var tfLink))
        {
            if (tfLink.IsBaseLink())
                tfLink.ResetOdomTransform();
            else 
                Debug.LogWarning($"TFLink on {vehicleName} is not a base_link, are you sure?");
        }
        else Debug.LogWarning($"No TFLink found on {vehicleName}, are you sure?");
    }
}
