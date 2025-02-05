using System;
using System.Collections;
using System.Collections.Generic;
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

    private SpriteRenderer sr;
    [HideInInspector] public InvaderState currentState = InvaderState.Single;

    [Header("In Love")]
    [SerializeField] private Sprite inLoveSprite;
    [SerializeField] private string inLoveSound;

    [Header("Taken")]
    [SerializeField] private Sprite takenSprite;
    [SerializeField] private string takenSound;


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
        AudioManager.instance.Play(inLoveSound);
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
            }
        }
    }

    public void SetTakenState()
    {
        currentState = InvaderState.Taken;
        sr.sprite = takenSprite;
        AudioManager.instance.Play(takenSound);
        Destroy(gameObject, 1f);
    }

    public void Shoot()
    {
        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
    }
}
