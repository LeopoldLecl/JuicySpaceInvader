using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float deadzone = 0.3f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [SerializeField] private float squashAmount = 0.8f; // Compression en Y lors de l'acc�l�ration
    [SerializeField] private float stretchAmount = 1.2f; // �tirement en Y lors de la d�c�l�ration
    [SerializeField] private float normalScale = 1f; // �chelle normale
    [SerializeField] private float stretchSpeed = 5f; // Vitesse d'interpolation

    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private string collideWithTag = "Untagged";

    private float lastShootTimestamp = Mathf.NegativeInfinity;
    private float currentSpeed = 0f;
    private float moveDirection = 0f;

    private Transform playerTransform;

    void Start()
    {
        playerTransform = transform; // R�cup�rer le transform pour optimisation
    }

    void Update()
    {
        UpdateMovement();
        UpdateActions();
        UpdateSquashStretchEffect();
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

        if (moveDirection != 0) // Acc�l�ration
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed * moveDirection, acceleration * Time.deltaTime);
        }
        else // D�c�l�ration
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
        }

        float delta = currentSpeed * Time.deltaTime;
        transform.position = GameManager.Instance.KeepInBounds(transform.position + Vector3.right * delta);
    }

    void UpdateSquashStretchEffect()
    {
        float targetScaleY = normalScale;

        if (Mathf.Abs(currentSpeed) > 0.1f) // Si le joueur bouge
        {
            if (Mathf.Abs(currentSpeed) < maxSpeed * 0.5f) // D�c�l�ration
                targetScaleY = normalScale * stretchAmount; // �tirement en Y
            else // Acc�l�ration max
                targetScaleY = normalScale * squashAmount; // Compression en Y
        }

        // Interpolation douce pour �viter les changements brusques
        Vector3 newScale = new Vector3(normalScale, Mathf.Lerp(playerTransform.localScale.y, targetScaleY, Time.deltaTime * stretchSpeed), normalScale);
        playerTransform.localScale = newScale;
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
