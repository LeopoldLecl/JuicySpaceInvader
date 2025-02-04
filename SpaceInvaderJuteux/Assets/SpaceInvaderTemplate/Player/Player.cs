using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float deadzone = 0.3f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f; 
    [SerializeField] private float deceleration = 15f;

    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private string collideWithTag = "Untagged";

    private float lastShootTimestamp = Mathf.NegativeInfinity;
    private float currentSpeed = 0f; 
    private float moveDirection = 0f; 

    void Update()
    {
        UpdateMovement();
        UpdateActions();
    }

    void UpdateMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (Mathf.Abs(moveInput) < deadzone)
        {
            moveDirection = 0;
        }
        else
        {
            moveDirection = Mathf.Sign(moveInput);
        }

        if (moveDirection != 0) //accelere
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed * moveDirection, acceleration * Time.deltaTime);
        }
        else //decelere
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
        }

        float delta = currentSpeed * Time.deltaTime;
        transform.position = GameManager.Instance.KeepInBounds(transform.position + Vector3.right * delta);
    }

    void UpdateActions()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time > lastShootTimestamp + shootCooldown)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
        lastShootTimestamp = Time.time;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != collideWithTag) { return; }

        GameManager.Instance.PlayGameOver();
    }
}
