using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearFreezeComponent : ClearablePiece
{

    //in aggiunta alle azioni della clear normale, si ridefinisce la funzione e si aggiunge
    //un'ulteriore chiamat per gli effetti della clear
    public override void Clear()
    {
        base.Clear();
        piece.GridRef.ClearFreeze();
    }
}
