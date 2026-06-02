using UnityEngine;

public class ConstantRotator : MonoBehaviour
{
    [Header("회전 속도")]
    public float rotationSpeed = 50f; // 숫자가 클수록 빠르게 돕니다.

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}