using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    public float accelerateFactor = 10;
    public float decelerateFactor = 7;
    public float moveSpeed = 3.7f;
    public float turnFactor = 12;
    public float deadZone = .05f;

    const string moveSpeedString = "MoveSpeed";
    int moveSpeedHash;

    CharacterController controller;
    Animator animator;
    Camera cam;

    float inputMagnitude, smoothedMagnitude;
    Vector3 inputDirection, targetDirection, smoothedDirection;
    Quaternion smoothedRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        moveSpeedHash = Animator.StringToHash(moveSpeedString);
        cam = Camera.main;
    }

    void Update()
    {
        SmoothSpeed();
        if (inputMagnitude > deadZone)
        {
            SmoothDirection();
            transform.rotation = smoothedRotation;
        }
        if (smoothedMagnitude > deadZone)
        {
            animator.SetFloat(moveSpeedHash, smoothedMagnitude);
            float sqCurrentMagnitude = smoothedMagnitude * smoothedMagnitude;
            controller.Move(smoothedDirection * sqCurrentMagnitude * moveSpeed * Time.deltaTime);
        }
    }

    public void SetInputVector(InputAction.CallbackContext value)
    {
        SetInputVector(value.ReadValue<Vector2>());
    }

    public void SetInputVector(Vector2 value)
    {
        inputMagnitude = value.magnitude;
        inputDirection = new Vector3(value.x, 0, value.y).normalized;
    }

    void SmoothSpeed()
    {
        float magnitudeDifference = inputMagnitude - smoothedMagnitude;
        float factor = (magnitudeDifference > 0 ? accelerateFactor : decelerateFactor) * Time.deltaTime;
        float lerpedMagnitude = Mathf.Lerp(smoothedMagnitude, inputMagnitude, factor);
        smoothedMagnitude = Mathf.Abs(magnitudeDifference) > .05f ? lerpedMagnitude : inputMagnitude;
    }

    void SmoothDirection()
    {
        Quaternion inputRotation = Quaternion.LookRotation(inputDirection);
        Quaternion cameraRotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
        Quaternion targetRotation = cameraRotation * inputRotation;
        smoothedRotation = Quaternion.Slerp(transform.rotation, targetRotation, turnFactor * Time.deltaTime);
        smoothedDirection = smoothedRotation * Vector3.forward;
        targetDirection = targetRotation * Vector3.forward;
    }
}
