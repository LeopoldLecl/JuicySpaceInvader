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
    [SerializeField] private Text coupleIdText;

    public InvaderState currentState = InvaderState.Single;
    public int coupleId;

    internal Action<Invader> onDestroy;

    public Vector2Int GridIndex { get; private set; }

    public void Initialize(Vector2Int gridIndex, int _coupleId)
    {
        this.GridIndex = gridIndex;
        coupleId = _coupleId;
        coupleIdText.text = coupleId.ToString();
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
        GetComponent<SpriteRenderer>().color = Color.white;
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
                Debug.Log("quoicoubeh");
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
                invader.currentState = InvaderState.Taken;
                Destroy(remainingInvader);
            }
        }
    }

    public void Shoot()
    {
        Instantiate(bulletPrefab, shootAt.position, Quaternion.identity);
    }
}
