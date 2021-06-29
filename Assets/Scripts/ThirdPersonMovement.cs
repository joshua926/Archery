using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    public float accelerateFactor = 10;
    public float decelerateFactor = 7;
    public float moveSpeed = 3.7f;
    public float turnSmoothTime = .12f;

    const string moveSpeedString = "MoveSpeed";
    int moveSpeedHash;

    PlayerInput playerInput;
    CharacterController controller;
    Animator animator;
    Camera cam;

    float inputMagnitude, currentMagnitude;
    Vector3 inputDirection;
    float inputAngle;
    float currentTurnVelocity;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        moveSpeedHash = Animator.StringToHash(moveSpeedString);
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentMagnitude != inputMagnitude)
        {
            currentMagnitude = SmoothMagnitude(currentMagnitude, inputMagnitude, accelerateFactor, decelerateFactor);
        }
        if (currentMagnitude > .1f)
        {
            animator.SetFloat(moveSpeedHash, currentMagnitude);
            float sqCurrentMagnitude = currentMagnitude * currentMagnitude;
            controller.Move(inputDirection * sqCurrentMagnitude * moveSpeed * Time.deltaTime);
        }
        if (inputMagnitude > .1f)
        {
            transform.rotation = SmoothAngle(inputAngle, cam.transform.eulerAngles.y, transform.eulerAngles.y, ref currentTurnVelocity, turnSmoothTime);
        }
    }

    public void SetInputVector(InputAction.CallbackContext value)
    {
        SetInputVector(value.ReadValue<Vector2>());
    }

    public void SetInputVector(Vector2 value)
    {
        inputMagnitude = value.magnitude;
        inputAngle = Mathf.Atan2(value.x, value.y) * Mathf.Rad2Deg;
        inputDirection = new Vector3(value.x, 0, value.y).normalized;
    }

    static float SmoothMagnitude(float currentMagnitude, float inputMagnitude, float accelerateFactor, float decelerateFactor)
    {
        float magnitudeDifference = inputMagnitude - currentMagnitude;
        if (Mathf.Abs(magnitudeDifference) < .05) { return inputMagnitude; }
        float factor = magnitudeDifference > 0 ? accelerateFactor : decelerateFactor;
        factor *= Time.deltaTime;
        return Mathf.Lerp(currentMagnitude, inputMagnitude, factor);
    }

    static Quaternion SmoothAngle(float inputAngle, float cameraAngle, float currentAngle, ref float currentTurnVelocity, float turnSmoothTime)
    {
        float targetAngle = inputAngle + cameraAngle;
        float smoothedAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentTurnVelocity, turnSmoothTime);
        return Quaternion.Euler(0, smoothedAngle, 0);
    }
}
