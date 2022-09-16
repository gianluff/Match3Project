using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearablePiece : MonoBehaviour
{
    public AnimationClip clearAnimation;
    private bool isBeingCleared = false;
    protected GamePiece piece;
    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }

    void Awake()
    {
        piece = GetComponent<GamePiece>();
    }

    //OnPieceCleared aggiorna i punteggi
    //ClearCoroutine avvia l'animazione associata al tipo di pezzo e poi lo distrugge
    public virtual void Clear()
    {
        piece.GridRef.game.OnPieceCleared(piece);
        isBeingCleared = true;
        StartCoroutine(ClearCoroutine());
    }
    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.Play(clearAnimation.name);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}
