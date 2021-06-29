using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterInputHandler : MonoBehaviour
{
    public float accelerateFactor = 10;
    public float decelerateFactor = 10;
    public float turnFactor = 6;
    public float moveSpeed = 3.66f;

    const string moveSpeedString = "MoveSpeed";
    int moveSpeedHash;
    Animator animator;
    CharacterController controller;

    float inputMagnitude, currentMagnitude;
    Vector2 inputDirection, currentDirection;

    void OnEnable()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        moveSpeedHash = Animator.StringToHash(moveSpeedString);
        currentDirection = new Vector2(transform.forward.x, transform.forward.z);
    }

    void Update()
    {
        if (inputMagnitude != currentMagnitude)
        {
            currentMagnitude = AnimationMagnitude(currentMagnitude, inputMagnitude, accelerateFactor, decelerateFactor);
            animator.SetFloat(moveSpeedHash, currentMagnitude);
        }
        if (inputDirection != Vector2.zero && inputDirection != currentDirection)
        {
            Quaternion animationRotation = AnimationRotation(currentDirection, inputDirection, turnFactor);
            transform.rotation = animationRotation; 
            currentDirection = AnimationDirection(animationRotation);
        }
        if (inputMagnitude > 0)
        {
            controller.Move(MoveVector(inputDirection, currentMagnitude, moveSpeed));
        }
    }

    public void SetInputVector(InputAction.CallbackContext value)
    {
        SetInputVector(value.ReadValue<Vector2>());
    }

    public void SetInputVector(Vector2 inputValue)
    {
        inputDirection = inputValue.normalized;
        inputMagnitude = inputValue.magnitude;        
    }

    static Vector3 MoveVector(Vector2 normalizedDirection, float magnitude, float speedMultiplier)
    {
        Vector3 moveDirection = new Vector3(normalizedDirection.x, 0, normalizedDirection.y);
        float sqMagnitude = magnitude * magnitude;
        return moveDirection * sqMagnitude * Time.deltaTime * speedMultiplier;
    }

    static float AnimationMagnitude(float currentMagnitude, float inputMagnitude, float accelerateFactor, float decelerateFactor)
    {
        float magnitudeDifference = inputMagnitude - currentMagnitude;
        if (Mathf.Abs(magnitudeDifference) < .05) { return inputMagnitude; }
        float factor = magnitudeDifference > 0 ? accelerateFactor : decelerateFactor;
        factor *= Time.deltaTime;
        return Mathf.Lerp(currentMagnitude, inputMagnitude, factor);
    }

    static Quaternion AnimationRotation(Vector2 currentDirection, Vector2 inputDirection, float turnFactor)
    {
        Vector3 current = new Vector3(currentDirection.x, 0, currentDirection.y);
        Vector3 input = new Vector3(inputDirection.x, 0, inputDirection.y);
        Quaternion currentRotation = Quaternion.LookRotation(current);
        Quaternion inputRotation = Quaternion.LookRotation(input);
        return Quaternion.Slerp(currentRotation, inputRotation, turnFactor * Time.deltaTime);
    }

    static Vector2 AnimationDirection(Quaternion rotation)
    {
        float angle = rotation.eulerAngles.y;
        angle *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));            
    }
}
