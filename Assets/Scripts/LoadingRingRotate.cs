using UnityEngine;

public class LoadingRingRotate : MonoBehaviour
{
    Vector3 rotationAxis = Vector3.up;
    float rotationSpeed = 500f;

    void Update()
    {
        float step = rotationSpeed * Time.deltaTime;
        transform.Rotate(rotationAxis, step, Space.Self);
    }
}
