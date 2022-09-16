using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearBombComponent : ClearablePiece
{

    //in aggiunta alle azioni della clear normale, si ridefinisce la funzione e si aggiunge
    //un'ulteriore chiamat per gli effetti della bomba
    public override void Clear()
    {
        base.Clear();
        piece.GridRef.ClearBomb(this.GetComponent<GamePiece>().X, this.GetComponent<GamePiece>().Y);
    }
}
