using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class PlayerMovement : MonoBehaviour
{
    Animator m_Animator;
    public InputAction MoveAction;

    public InputAction JumpAction;

    public InputAction RunAction;

    public float jumpForce = 5f;
    public float groundCheckDistance = 1.1f;

    bool isGrounded;

    public float walkSpeed = 1.0f;
    public float runSpeed = 4.0f;
    public float turnSpeed = 20f;

    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    [Header("Jump Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip landingSound;

    private bool wasGrounded;
    private bool hasJumped;

    // inventory tracking/object interaction
    public int keyNo; // tracks amount of keys held by player
    public bool hasBarrel; //tracks if player is holding a barrel
    public int playerScore;
    public int switch1;
    public int switch2;
    public int i;
    public int clues;
    [Header("Code System")]
    public CodeController codeController;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        keyNo = 0;
        playerScore = 0;
        switch1 = 0;
        switch2 = 0;
        i = 0;
        clues = 0;

        MoveAction.Enable();
        JumpAction.Enable();
        RunAction.Enable();
    }

    void Update()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        if (JumpAction.WasPressedThisFrame() && isGrounded)
        {
            hasJumped = true;

            m_Rigidbody.AddForce(
                Vector3.up * jumpForce,
                ForceMode.Impulse
            );

            m_Animator.SetTrigger("Jump");

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }
        }

        if (isGrounded && !wasGrounded && hasJumped)
        {
            if (audioSource != null && landingSound != null)
            {
                audioSource.PlayOneShot(landingSound);
            }

            hasJumped = false;
        }

        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        var pos = MoveAction.ReadValue<Vector2>();

        float horizontal = pos.x;
        float vertical = pos.y;

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("IsWalking", isWalking);

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);

        m_Rigidbody.MoveRotation(m_Rotation);
        float currentSpeed = RunAction.IsPressed() ? runSpeed : walkSpeed;

        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * currentSpeed * Time.deltaTime);
    }

    public void CollectClue(int clueNumber)
    {
        codeController.RevealClue(clueNumber);
    }

    private void OnTriggerEnter(Collider other) //checks collision between player and other objects
    {
        if (other.CompareTag("Key")) //adds key to inventory when colliding with one
        {
            keyNo += 1;
            Destroy(other.gameObject);
        }
        if (other.CompareTag("KeyDoor") && keyNo > 0)
        {
            keyNo -= 1;
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Sign"))
        {
            //code to read sign to be added
        }
        if (other.CompareTag("Barrel"))
        {
            hasBarrel = true;
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Plate") && hasBarrel == true)
        {
            //spawn.spawnBarrel();
            if (i == 0)
            {
                Instantiate(GameObject.Find("Barrel2"), GameObject.Find("PressurePlate1").transform.position, transform.rotation);
                Destroy(other.gameObject);
                hasBarrel = false;
                i += 1;
            }
            else if (i == 1)
            {
                Instantiate(GameObject.Find("Barrel2"), GameObject.Find("PressurePlate2").transform.position, transform.rotation);
                Destroy(other.gameObject);
                hasBarrel = false;
                i += 1;
            }
            else
            {
                Instantiate(GameObject.Find("Barrel2"), GameObject.Find("PressurePlate3").transform.position, transform.rotation);
                Destroy(other.gameObject);
                hasBarrel = false;
                i += 1;
            }

        }
        if (other.CompareTag("Chest"))
        {
            playerScore += 100;
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Switch1"))
        {
            switch1 += 1;
            other.tag = "Untagged";
        }
        if (other.CompareTag("SDoor1") && switch1 == 3)
        {
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Switch2"))
        {
            switch2 += 1;
            other.tag = "Untagged";
        }
        if (other.CompareTag("SDoor2") && switch2 == 2)
        {
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Switch3"))
        {
            Destroy(GameObject.Find("ChestDoor"));
        }
        if (other.CompareTag("Sign"))
        {
            clues += 1;
            other.tag = "Untagged";
        }
        if (other.CompareTag("Exit") && clues == 4)
        {
            //win condition
        }
    }
}