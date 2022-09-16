using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int score;
    private int x;
    private int y;
    private Griglia.PType type;
    private Griglia grid;
    private MovablePiece movableComponent;
    private ColorPiece colorComponent;
    private ClearablePiece clearableComponent;

    public int X
    {
        get { return x;  }
        set
        {
            if (IsMovable())
            {
                x = value;
            }
        }
    }

    public int Y
    {
        get { return y; }
        set
        {
            if (IsMovable())
            {
                y = value;
            }
        }
    }

    public Griglia.PType Type
    {
        get { return type; }
    }

    public Griglia GridRef
    {
        get { return grid; }
    }

    public MovablePiece MovableComponent
    {
        get { return movableComponent; }
    }

    public ColorPiece ColorComponent
    {
        get { return colorComponent; }
    }

    public ClearablePiece ClearableComponent
    {
        get { return clearableComponent; }
    }

    void Awake()
    {
        movableComponent = GetComponent<MovablePiece>();
        colorComponent = GetComponent<ColorPiece>();
        clearableComponent = GetComponent<ClearablePiece>();
    }

    //inizializzo i campi del pezzo, posizione, griglia e tipo
    public void Initialize(int _x, int _y, Griglia _grid, Griglia.PType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;
    }
    //funzioni che rilevano quando clicco o tocco un pezzo
    //o quando lo rilascio. In caso di rilascio eseguirò lo swap
    //se i pezzi sono adiacenti
    void OnMouseEnter()
    {
        grid.EnterPiece(this);
    }

    void OnMouseDown()
    {
        grid.PressPiece(this);
    }

    void OnMouseUp()
    {
        grid.ReleasePiece();
    }
    //IsMovable, IsColored, IsClearable verificano la presenza degli omologhi componenti
    //discriminano l'eventualità di avere a che fare con un pezzo "valido" o con uno Empty
    //privo dei suddetti componenti 
    public bool IsMovable()
    {
        if (movableComponent != null)
            return true;
        else
            return false;
    }

    public bool IsColored()
    {
        if (colorComponent != null)
            return true;
        else
            return false;
    }

    public bool IsClearable()
    {
        if (clearableComponent != null)
            return true;
        else
            return false;
    }
}
