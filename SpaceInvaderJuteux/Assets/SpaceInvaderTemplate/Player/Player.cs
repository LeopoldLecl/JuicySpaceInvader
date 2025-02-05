using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float deadzone = 0.3f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [SerializeField] private float squashAmount = 0.8f; // Compression en Y lors de l'accélération
    [SerializeField] private float stretchAmount = 1.2f; // Étirement en Y lors de la décélération
    [SerializeField] private float normalScale = 1f; // Échelle normale
    [SerializeField] private float stretchSpeed = 5f; // Vitesse d'interpolation

    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private float shootCooldown = 1f;
    [SerializeField] private string collideWithTag = "Untagged";

    private float lastShootTimestamp = Mathf.NegativeInfinity;
    private float currentSpeed = 0f;
    private float moveDirection = 0f;

    [Header("Recoil Effect")]
    [SerializeField] private float recoilAmplitude = 0.2f;
    [SerializeField] private float dampingFactor = 2.0f;
    [SerializeField] private float frequency = 10f;
    private Vector3 recoilOffset = Vector3.zero;

    private Transform playerTransform;

    void Start()
    {
        playerTransform = transform; // Récupérer le transform pour optimisation
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

        if (moveDirection != 0) // Accélération
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed * moveDirection, acceleration * Time.deltaTime);
        }
        else // Décélération
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
            if (Mathf.Abs(currentSpeed) < maxSpeed * 0.5f) // Décélération
                targetScaleY = normalScale * stretchAmount; // Étirement en Y
            else // Accélération max
                targetScaleY = normalScale * squashAmount; // Compression en Y
        }

        // Interpolation douce pour éviter les changements brusques
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
        StartCoroutine(RecoilEffect());
    }

    IEnumerator RecoilEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shootCooldown)
        {
            float displacement = recoilAmplitude * Mathf.Exp(-dampingFactor * elapsedTime) * Mathf.Sin(frequency * elapsedTime);
            Vector3 newOffset = new Vector3(0, -displacement, 0);
            transform.position += newOffset - recoilOffset;
            recoilOffset = newOffset;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position -= recoilOffset;
        recoilOffset = Vector3.zero;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != collideWithTag) { return; }

        GameManager.Instance.PlayGameOver();
    }
}
