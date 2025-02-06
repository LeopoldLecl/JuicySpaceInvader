using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Invader : MonoBehaviour
{
    public enum InvaderState
    {
        Single,
        InLove,
        Taken
    }

    [SerializeField] private Bullet bulletPrefab = null;
    [SerializeField] private Transform shootAt = null;
    [SerializeField] private string collideWithTag = "Player";
    public int coupleId;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private Sprite[] sadSprites;

    private SpriteRenderer sr;
    [HideInInspector] public InvaderState currentState = InvaderState.Single;

    [Header("Shoot")]
    [SerializeField] private string[] enemyShootSounds;

    [Header("In Love")]
    [SerializeField] private Sprite[] inLoveSprites;
    [SerializeField] private string[] inLoveSounds;

    [Header("Taken")]
    [SerializeField] private Sprite[] takenSprites;
    [SerializeField] private string[] takenSounds;
    [SerializeField] private GameObject takenVfxPrefab;

    [SerializeField] private float ScreenShakeIntensity = 0.04f;
    [SerializeField] private float ScreenShakeDuration = 0.05f;
    [SerializeField] private AnimationCurve ScreenShakeCurve = AnimationCurve.Linear(0, 0, 1, 1);


    internal Action<Invader> onDestroy;

    public Vector2Int GridIndex { get; private set; }

    public void Initialize(Vector2Int gridIndex, int _coupleId)
    {
        this.GridIndex = gridIndex;
        coupleId = _coupleId;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sadSprites[coupleId -1];
    }

    public void OnDestroy()
    {
        onDestroy?.Invoke(this);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag != collideWithTag) { return; }
        UpdateInvaderState();
        if (GameManager.Instance.vfx3Enabled)
        {
            GameObject impact = Instantiate(impactPrefab, collision.transform.position, collision.transform.rotation);
            Destroy(impact, 4f);
        }
        Destroy(collision.gameObject);
        ScreenShake.instance.ShakeScreenWithCurve(Camera.main, ScreenShakeIntensity, ScreenShakeDuration, ScreenShakeCurve);
    }

    void UpdateInvaderState()
    {
        switch (currentState)
        {
            case InvaderState.Single:
                SetInLoveState();
                if (TestTakenState(coupleId))
                {
                    SetTakenStateForAll(coupleId);
                }
            break;
        }
    }

    void SetInLoveState()
    {
        currentState = InvaderState.InLove;
        sr.sprite = inLoveSprites[coupleId -1];
        AudioManager.instance.PlayRandom(inLoveSounds);
    }

    bool TestTakenState(int _coupleId)
    {
        bool coupleIsComplete = true;
        GameObject[] remainingInvaders = GameObject.FindGameObjectsWithTag("Invader");
        foreach (GameObject remainingInvader in remainingInvaders)
        {
            Invader invader = remainingInvader.GetComponent<Invader>();
            if (invader.coupleId == _coupleId && invader.currentState != InvaderState.InLove)
            {
                coupleIsComplete = false;
            }
        }
        return coupleIsComplete;
    }

    void SetTakenStateForAll(int _coupleId)
    {
        GameObject[] remainingInvaders = GameObject.FindGameObjectsWithTag("Invader");
        foreach (GameObject remainingInvader in remainingInvaders)
        {
            Invader invader = remainingInvader.GetComponent<Invader>();
            if (invader.coupleId == _coupleId)
            {
                invader.SetTakenState();
                GameManager.Instance.AddScore(10);
            }
        }
    }

    public void SetTakenState()
    {
        currentState = InvaderState.Taken;
        sr.sprite = takenSprites[coupleId -1];
        AudioManager.instance.PlayRandom(takenSounds);
        if (GameManager.Instance.vfx6Enabled)
        {
            GameObject vfx = Instantiate(takenVfxPrefab, transform);
            //Destroy(vfx, 4f);
        }
        transform.parent.gameObject.GetComponent<Wave>().RemoveInvader(this);
        transform.parent = null;
        Vector3 finalPosition = GameManager.Instance.GetFinalPosition(GameManager.Instance.takenFrogCount);
        GameManager.Instance.takenFrogCount++;
        StartCoroutine(MoveCoroutine(finalPosition, 5f));
    }



    private IEnumerator MoveCoroutine(Vector3 targetPosition, float speed)
    {
        Vector3 targetScale = new Vector3(0.4f, 0.4f, 0.4f);
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }

    public void Shoot()
    {
        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
        AudioManager.instance.PlayRandom(enemyShootSounds);
    }
}
