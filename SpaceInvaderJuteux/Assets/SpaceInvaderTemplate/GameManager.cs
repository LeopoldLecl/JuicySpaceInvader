using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public enum DIRECTION { Right = 0, Up = 1, Left = 2, Down = 3 }

    public static GameManager Instance = null;

    [SerializeField] private Vector2 bounds;
    private Bounds Bounds => new Bounds(transform.position, new Vector3(bounds.x, bounds.y, 1000f));

    [SerializeField] private float gameOverHeight;
    [SerializeField] private string themeName;

    public int score = 0;

    public int takenFrogCount = 0;


    public CoupleDatas coupleDatas;

    public Transform StartGrassFrogPoint;
    public Transform EndGrassFrogPoint;
    [SerializeField] private GameObject confettisPrefab;

    [Header("Flowers")]
    [SerializeField] SpriteRenderer flowersSr;
    [SerializeField] Sprite secondFlowersSprite;
    [SerializeField] Sprite thirdFlowersSprite;

    [Header("Enable VFX")]
    public bool vfx1Enabled = true;
    [SerializeField] GameObject movementWind;
    public bool vfx2Enabled = true;
    public bool vfx3Enabled = true;
    public bool vfx4Enabled = true;
    public bool vfx5Enabled = true;
    public bool vfx6Enabled = true;
    public bool vfx7Enabled = true;
    public bool vfx8Enabled = true;
    public bool vfx9Enabled = true;


    private bool isStart = true;
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AudioManager.instance.Play(themeName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            vfx1Enabled = !vfx1Enabled;
            movementWind.SetActive(vfx1Enabled);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
            vfx2Enabled = !vfx2Enabled;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            vfx3Enabled = !vfx3Enabled;
        if (Input.GetKeyDown(KeyCode.Alpha4))
            vfx4Enabled = !vfx4Enabled;
        if (Input.GetKeyDown(KeyCode.Alpha5))
            vfx5Enabled = !vfx5Enabled;
        if (Input.GetKeyDown(KeyCode.Alpha6))
            vfx6Enabled = !vfx6Enabled;
        if (Input.GetKeyDown(KeyCode.Alpha7))
            vfx7Enabled = !vfx7Enabled;
        if (Input.GetKeyDown(KeyCode.Alpha8))
            vfx8Enabled = !vfx8Enabled;
        if (Input.GetKeyDown(KeyCode.Alpha9))
            vfx9Enabled = !vfx9Enabled;


        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Time.timeScale = 1f;

        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isStart) { Time.timeScale = 0f; isStart = false; }
            else { Time.timeScale = 1f; isStart = true; }
        }
    }

    public Vector3 KeepInBounds(Vector3 position)
    {
        return Bounds.ClosestPoint(position);
    }

    public float KeepInBounds(float position, DIRECTION side)
    {
        switch (side)
        {
            case DIRECTION.Right: return Mathf.Min(position, Bounds.max.x);
            case DIRECTION.Up: return Mathf.Min(position, Bounds.max.y);
            case DIRECTION.Left: return Mathf.Max(position, Bounds.min.x);
            case DIRECTION.Down: return Mathf.Max(position, Bounds.min.y);
            default: return position;
        }
    }

    public bool IsInBounds(Vector3 position)
    {
        return Bounds.Contains(position);
    }

    public bool IsInBounds(Vector3 position, DIRECTION side)
    {
        switch (side)
        {
            case DIRECTION.Right: case DIRECTION.Left: return IsInBounds(position.x, side);
            case DIRECTION.Up: case DIRECTION.Down: return IsInBounds(position.y, side);
            default: return false;
        }
    }

    public bool IsInBounds(float position, DIRECTION side)
    {
        switch (side)
        {
            case DIRECTION.Right: return position <= Bounds.max.x;
            case DIRECTION.Up: return position <= Bounds.max.y;
            case DIRECTION.Left: return position >= Bounds.min.x;
            case DIRECTION.Down: return position >= Bounds.min.y;
            default: return false;
        }
    }

    public bool IsBelowGameOver(float position)
    {
        return position < transform.position.y + (gameOverHeight - bounds.y * 0.5f);
    }

    public void PlayGameOver()
    {
        AudioManager.instance.Stop("BackGround Music");
        if (vfx8Enabled)
        {
            Player player = FindObjectOfType<Player>();
            player.targetVignetteIntensity = 0.7f;
            player.colorVfx.saturation.value = -100f;
            player.colorVfx.contrast.value = 68f;
            AudioManager.instance.Play("Death");
            AudioManager.instance.Play("Losing Music");
        }
        Debug.Log("Game Over");
        Time.timeScale = 0f;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position, new Vector3(bounds.x, bounds.y, 0f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position + Vector3.up * (gameOverHeight - bounds.y * 0.5f) - Vector3.right * bounds.x * 0.5f,
            transform.position + Vector3.up * (gameOverHeight - bounds.y * 0.5f) + Vector3.right * bounds.x * 0.5f);
    }

    public void AddScore(int _score)
    {
        score += _score;
        if (score == 80)
        {
            flowersSr.sprite = secondFlowersSprite;
        }
        else if (score == 160)
        {
            flowersSr.sprite = thirdFlowersSprite;
        }
        else if (score == 240)
        {
            AudioManager.instance.Stop("BackGround Music");
            if (!vfx7Enabled)
            {
                Debug.Log("You Win");
                Time.timeScale = 0f;
            }
            else
            {
                StartCoroutine(WinningCoroutine());
            }
        }
    }

    public Vector3 GetFinalPosition(int count)
    {
        float newX = Mathf.Lerp(StartGrassFrogPoint.position.x, EndGrassFrogPoint.position.x, (float)count / 24);
        float yVariation = (count % 2 == 0) ? 0.4f : -0.4f;
        return new Vector3(newX, StartGrassFrogPoint.position.y + (float)yVariation, 0);
    }

    private IEnumerator WinningCoroutine()
    {
        Player player = FindObjectOfType<Player>();
        player.isInGame = false;
        AudioManager.instance.Play("Winning Music");
        GameObject confettis1 = Instantiate(confettisPrefab, player.transform.position + new Vector3(2f, 2f, 0f), player.transform.rotation);
        GameObject confettis2 = Instantiate(confettisPrefab, player.transform.position + new Vector3(-2f, 2f, 0f), player.transform.rotation);
        Destroy(confettis1, 2.7f);
        Destroy(confettis2, 2.7f);
        yield return new WaitForSeconds(2.5f);

        Vector3 targetScale = new Vector3(6f, 6f, 6f);
        while (Vector3.Distance(player.transform.position, Vector3.zero) > 0.01f)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, Vector3.zero, 1f * Time.deltaTime);
            player.transform.localScale = Vector3.Lerp(player.transform.localScale, targetScale, 1f * Time.deltaTime);
            yield return null;
        }
        player.transform.localScale = targetScale;

        float elapsedTime = 0f;
        float danceSpeed = 2f;
        float amplitude = 30f;

        while (true)
        {
            elapsedTime += Time.deltaTime * danceSpeed;
            float newZ = Mathf.Lerp(-amplitude, amplitude, Mathf.PingPong(elapsedTime, 1f));
            player.transform.rotation = Quaternion.Euler(0, 0, newZ);
            yield return null;
        }
    }

}
