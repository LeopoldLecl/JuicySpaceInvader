using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Unity.Collections.AllocatorManager;


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
  //  [SerializeField] private string collideWithTag = "Untagged";

    private float lastShootTimestamp = Mathf.NegativeInfinity;
    private float currentSpeed = 0f;
    private float moveDirection = 0f;

    private Transform playerTransform;
    private int playerHealth = 3;

    //sfx
    private Volume volume;
    private Vignette vignetteVfx;
    private ColorAdjustments colorVfx;

    void Start()
    {
        playerTransform = transform; // Récupérer le transform pour optimisation
        volume = Camera.main.GetComponent<Volume>();

        if (volume == null)
        {
            Debug.LogError("Aucun Volume trouvé sur la caméra !");
            return;
        }
        if (volume.profile.TryGet<Vignette>(out vignetteVfx))
        {
            Debug.Log("Vignette trouvé !");
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
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            playerHealth--;

            switch(playerHealth) {
                case 2:
                    Debug.Log("Player health: " + playerHealth);
                    vignetteVfx.intensity.value = 0.4f;
                    break;
                case 1:
                    vignetteVfx.intensity.value = 0.5f;
                    Debug.Log("Player health: " + playerHealth);    
                    break;
                case 0:
                    colorVfx.saturation.value = -100f;
                    colorVfx.contrast.value = 68f;
                    GameManager.Instance.PlayGameOver();
                    break;
            }
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Invader"))
        {
            colorVfx.saturation.value = -100f;
            colorVfx.contrast.value = 68f;
            GameManager.Instance.PlayGameOver();
        }
    }
}
