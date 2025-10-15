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
    [SerializeField] Transform TransformToTeleport;
    [Tooltip("Expected location to teleport in.")]
    [SerializeField] Transform ExpectedLocation;

    [SerializeField] RandomGenerator horizontalRange;
    [SerializeField] RandomGenerator forwardRange;
    [SerializeField] RandomGenerator yawRange;

    // Start is called before the first frame update
    void Start()
    {
        Rigidbody[] RBparts = TransformToTeleport.gameObject.GetComponentsInChildren<Rigidbody>();

        var unityPosi = new Vector3(
                        ExpectedLocation.position.x + horizontalRange.RandomValue(),
                        ExpectedLocation.position.y,
                        ExpectedLocation.position.z + forwardRange.RandomValue());
        var unityOri = Quaternion.Euler(
                        ExpectedLocation.rotation.eulerAngles.x,
                        ExpectedLocation.rotation.eulerAngles.y + yawRange.RandomValue(),
                        ExpectedLocation.rotation.eulerAngles.z);

        TransformToTeleport.SetPositionAndRotation(unityPosi, unityOri);

        foreach (var rb in RBparts)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
