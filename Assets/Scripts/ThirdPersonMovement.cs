using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    public float accelerateFactor = 10;
    public float decelerateFactor = 7;
    public float moveSpeed = 3.7f;
    public float turnSpeed = 10;
    public float deadZone = .05f;

    const string moveSpeedString = "MoveSpeed";
    int moveSpeedHash;

    CharacterController controller;
    Animator animator;
    Camera cam;

    float inputMagnitude, smoothedMagnitude, animateMagnitude;
    Vector3 inputDirection, smoothedDirection;
    float currentTurnVelocity;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        moveSpeedHash = Animator.StringToHash(moveSpeedString);
        cam = Camera.main;
    }

    void Update()
    {
        SmoothMagnitude();
        SmoothDirection();
        Animate();
        Turn();
        Move();
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

    void SmoothMagnitude()
    {
        float difference = inputMagnitude - smoothedMagnitude;
        float factor = (difference > 0 ? accelerateFactor : decelerateFactor) * Time.deltaTime;
        float lerpedMagnitude = Mathf.Lerp(smoothedMagnitude, inputMagnitude, factor);
        bool bigDifference = Mathf.Abs(difference) > .05f;
        smoothedMagnitude = bigDifference ? lerpedMagnitude : inputMagnitude;
    }

    void SmoothDirection()
    {
        if (inputMagnitude < deadZone) { return; }
        float currentAngle = transform.eulerAngles.y;
        float inputLocalAngle = Quaternion.LookRotation(inputDirection).eulerAngles.y;
        float targetAngle = inputLocalAngle + cam.transform.eulerAngles.y;
        float smoothedAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentTurnVelocity, 1 / turnSpeed);
        Quaternion smoothedRotation = Quaternion.Euler(0, smoothedAngle, 0);
        smoothedDirection = smoothedRotation * Vector3.forward;
    }

    void Animate()
    {
        if (animateMagnitude == smoothedMagnitude) { return; }
        animateMagnitude = smoothedMagnitude;
        animator.SetFloat(moveSpeedHash, animateMagnitude);
    }

    void Turn()
    {
        if (inputMagnitude < deadZone) { return; }
        transform.rotation = Quaternion.LookRotation(smoothedDirection);
    }

    void Move()
    {
        if (inputMagnitude < deadZone) { return; }
        float sqCurrentMagnitude = smoothedMagnitude * smoothedMagnitude;
        controller.Move(smoothedDirection * sqCurrentMagnitude * moveSpeed * Time.deltaTime);
    }
}
