using UnityEngine;

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
    [Tooltip("The object to teleport around. Can handle Arti. Bodies too.")]
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
    }
}
