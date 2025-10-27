using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Perception.ROS;

public class ConfidenceRateInputUI : MonoBehaviour
{
    [SerializeField] private BoundingBoxPublisher publisher;
    [SerializeField] private TMP_InputField inputField;

    void Start()
    {
        inputField.text = publisher.ConfidenceRate.ToString("0.00");
        inputField.onEndEdit.AddListener(OnConfidenceChanged);
    }

    public void OnConfidenceChanged(string text)
    {
        if (float.TryParse(text, out float val))
            publisher.ConfidenceRate = val;
        inputField.text = publisher.ConfidenceRate.ToString("0.00");
    }
}
