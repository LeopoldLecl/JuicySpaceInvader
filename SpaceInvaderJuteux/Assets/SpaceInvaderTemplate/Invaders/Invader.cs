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
    [SerializeField] private Text coupleIdText;
    [SerializeField] private GameObject impactPrefab;

    private SpriteRenderer sr;
    [HideInInspector] public InvaderState currentState = InvaderState.Single;

    [Header("Shoot")]
    [SerializeField] private string[] enemyShootSounds;

    [Header("In Love")]
    [SerializeField] private Sprite inLoveSprite;
    [SerializeField] private string[] inLoveSounds;

    [Header("Taken")]
    [SerializeField] private Sprite takenSprite;
    [SerializeField] private string[] takenSounds;
    [SerializeField] private GameObject takenVfxPrefab;


    internal Action<Invader> onDestroy;

    public Vector2Int GridIndex { get; private set; }

    public void Initialize(Vector2Int gridIndex, int _coupleId)
    {
        this.GridIndex = gridIndex;
        coupleId = _coupleId;
        coupleIdText.text = coupleId.ToString();
        sr = GetComponent<SpriteRenderer>();
    }

    public void OnDestroy()
    {
        onDestroy?.Invoke(this);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag != collideWithTag) { return; }
        UpdateInvaderState();
        GameObject impact = Instantiate(impactPrefab, collision.transform.position, collision.transform.rotation);
        Destroy(impact, 4f);
        Destroy(collision.gameObject);
        ScreenShake.instance.ShakeScreen(Camera.main,0.1f, 0.05f);
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
        sr.sprite = inLoveSprite;
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
        sr.sprite = takenSprite;
        AudioManager.instance.PlayRandom(takenSounds);
        GameObject vfx = Instantiate(takenVfxPrefab, transform.position, transform.rotation);
        Destroy(vfx, 4f);
        //Destroy(gameObject, 2.5f);
        transform.parent = null;
        Vector3 finalPosition = GameManager.Instance.GetFinalPosition(GameManager.Instance.takenFrogCount);
        GameManager.Instance.takenFrogCount++;
        StartCoroutine(MoveCoroutine(finalPosition, 5f));
    }



    private IEnumerator MoveCoroutine(Vector3 targetPosition, float speed)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
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
