using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{
    private GamePiece piece;
    private IEnumerator moveCoroutine;
    void Awake()
    {
        piece = GetComponent<GamePiece>();
    }

    //per il movimento uso una Coroutine per creare un'animazione
    public void Move(int landingX, int landingY, float time)
    {

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = MoveCoroutine(landingX, landingY, time);
        StartCoroutine(moveCoroutine);
    }

    //setto le nuove coordinate del pezzo e con GetWord GetWorldPosition calcolo
    //la sua posizione nella scena. L'animazione è ottenuta con Lerp, interpolando la
    //posizione del pezzo tra il punto di partenza e quello di arrivo
    private IEnumerator MoveCoroutine(int landingX, int landingY, float time)
    {
        piece.X = landingX;
        piece.Y = landingY;

        Vector3 startPos = transform.position;
        Vector3 endPos = piece.GridRef.GetWorldPosition(landingX, landingY);
        for (float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0;
        }

        piece.transform.position = endPos;
    }
}
