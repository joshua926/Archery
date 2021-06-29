using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovementHandler : MonoBehaviour
{
    [Tooltip("In meters per second squared.")]
    public float acceleration = 4, deceleration = -4;
    [Tooltip("In radians per second")]
    public float turnSpeed = 6;
    public float moveSpeed = 3.66f;
    public Vector2 inputRamp = new Vector2(.2f, 1);

    const string moveSpeedString = "MoveSpeed";
    Animator animator;
    int moveSpeedHash;

    CharacterController controller;

    float inputMagnitude, currentMagnitude;
    float inputRadian, currentRadian;

    void Awake()
    {
        moveSpeedHash = Animator.StringToHash(moveSpeedString);
        currentRadian = ToRadian(transform.forward);
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (inputMagnitude != currentMagnitude)
        {
            currentMagnitude = NewMagnitude(currentMagnitude, inputMagnitude, acceleration, deceleration);
            animator.SetFloat(moveSpeedHash, currentMagnitude);
        }
        if (inputRadian != currentRadian)
        {
            currentRadian = NewRadians(currentRadian, inputRadian, turnSpeed);
            transform.rotation = ToQuaternion(currentRadian);
        }
        if (inputMagnitude > 0)
        {
            controller.Move(CalculateMoveVector(inputRadian, currentMagnitude, moveSpeed));
        }
    }

    public void SetInputVector(InputAction.CallbackContext value)
    {
        SetInputVector(value.ReadValue<Vector2>());
    }

    public void SetInputVector(Vector2 inputValue)
    {
        inputRadian = ToRadian(inputValue);
        inputMagnitude = inputValue.magnitude;
        if (inputMagnitude > .01f)
        {
            float rampRange = inputRamp.y - inputRamp.x;
            inputMagnitude = inputMagnitude * rampRange + inputRamp.x;
        }
    }

    static Vector3 CalculateMoveVector(float radian, float magnitude, float speedMultiplier)
    {
        Vector2 dir = ToNormalizedVector(radian);
        Vector3 moveDirection = new Vector3(dir.x, 0, dir.y);
        float moveMagnitude = magnitude * magnitude;
        moveMagnitude = moveMagnitude >= .3f ? moveMagnitude : 0;
        return moveDirection * moveMagnitude * Time.deltaTime * speedMultiplier;
    }

    static float NewMagnitude(float currentMagnitude, float inputMagnitude, float acceleration, float deceleration)
    {
        acceleration *= Time.deltaTime;
        deceleration *= Time.deltaTime;
        float magnitudeDifference = inputMagnitude - currentMagnitude;
        float deltaMagnitude =
            inputMagnitude > currentMagnitude ?
            Mathf.Min(acceleration, magnitudeDifference) :
            Mathf.Max(deceleration, magnitudeDifference);
        return Mathf.Clamp(currentMagnitude + deltaMagnitude, 0, 1);
    }

    static float NewRadians(float currentRadians, float inputRadians, float radiansPerSecond)
    {
        float difference = AngleDifference(inputRadians, currentRadians);
        float perFrameTurn = (difference > 0 ? radiansPerSecond : -radiansPerSecond) * Time.deltaTime;
        return currentRadians + Mathf.Abs(difference) < Mathf.Abs(perFrameTurn) ? difference : perFrameTurn;
    }

    static float AngleDifference(float a, float b)
    {
        float difference = a - b;
        float fullCircle = 4 * Mathf.PI;
        difference %= fullCircle;
        difference = difference > fullCircle / 2 ? difference - fullCircle : difference;
        difference = difference < -fullCircle / 2 ? difference + fullCircle : difference;
        return difference;
    }

    static Quaternion ToQuaternion(float radian)
    {
        Vector2 currentDirection = ToNormalizedVector(radian);
        return Quaternion.LookRotation(new Vector3(currentDirection.x, 0, currentDirection.y), Vector3.up);
    }

    static float ToRadian(Vector3 dir)
    {
        return ToRadian(new Vector2(dir.x, dir.z));
    }

    static float ToRadian(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x);
    }

    static Vector2 ToNormalizedVector(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
}