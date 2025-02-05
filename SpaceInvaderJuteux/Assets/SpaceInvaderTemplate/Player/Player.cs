using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Player : MonoBehaviour
{
    [SerializeField] private float deadzone = 0.3f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [SerializeField] private float squashAmount = 0.8f;
    [SerializeField] private float stretchAmount = 1.2f;
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float stretchSpeed = 5f;

    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private float shootCooldown = 1f;

    private float lastShootTimestamp = Mathf.NegativeInfinity;
    private float currentSpeed = 0f;
    private float moveDirection = 0f;

    private Transform playerTransform;
    private int playerHealth = 3;

    // Post-processing effects
    private Volume volume;
    private Vignette vignetteVfx;
    private ColorAdjustments colorVfx;

    private float targetVignetteIntensity = 0f; 
    private float vignetteLerpSpeed = 10f; 
    void Start()
    {
        playerTransform = transform;
        volume = Camera.main.GetComponent<Volume>();

        if (volume == null)
        {
            Debug.LogError("Aucun Volume trouvé sur la caméra !");
            return;
        }
        if (volume.profile.TryGet<Vignette>(out vignetteVfx))
        {
            Debug.Log("Vignette trouvée !");
            vignetteVfx.intensity.value = 0f; // Désactiver au départ
        }
        if (volume.profile.TryGet<ColorAdjustments>(out colorVfx))
        {
            Debug.Log("ColorAdjustments trouvé !");
        }
    }

    void Update()
    {
        UpdateMovement();
        UpdateActions();
        UpdateSquashStretchEffect();
        SmoothVignetteEffect(); // Applique l'interpolation du vignettage
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

        if (moveDirection != 0)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed * moveDirection, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
        }

        float delta = currentSpeed * Time.deltaTime;
        transform.position = GameManager.Instance.KeepInBounds(transform.position + Vector3.right * delta);
    }

    void UpdateSquashStretchEffect()
    {
        float targetScaleY = normalScale;

        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            if (Mathf.Abs(currentSpeed) < maxSpeed * 0.5f)
                targetScaleY = normalScale * stretchAmount;
            else
                targetScaleY = normalScale * squashAmount;
        }

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

    // --- NOUVELLE FONCTION POUR INTERPOLATION DU VIGNETTAGE ---
    void SmoothVignetteEffect()
    {
        if (vignetteVfx != null)
        {
            vignetteVfx.intensity.value = Mathf.Lerp(vignetteVfx.intensity.value, targetVignetteIntensity, Time.deltaTime * vignetteLerpSpeed);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            playerHealth--;

            switch (playerHealth)
            {
                case 2:
                    Debug.Log("Player health: " + playerHealth);
                    targetVignetteIntensity = 0.45f; 
                    break;
                case 1:
                    Debug.Log("Player health: " + playerHealth);
                    targetVignetteIntensity = 0.55f;
                    break;
                case 0:
                    targetVignetteIntensity = 0.7f;
                    colorVfx.saturation.value = -100f;
                    colorVfx.contrast.value = 68f;
                    GameManager.Instance.PlayGameOver();
                    break;
            }
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Invader"))
        {
            targetVignetteIntensity = 0.7f;
            colorVfx.saturation.value = -100f;
            colorVfx.contrast.value = 68f;
            GameManager.Instance.PlayGameOver();
        }
    }
}
