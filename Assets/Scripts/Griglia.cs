using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Griglia : MonoBehaviour
{
    public int X;
    public int Y;
    public float fillTime;
    private GamePiece pressedPiece;
    private GamePiece enteredPiece;
    private bool gameOver = false;
    public static bool stopTimer = false;
    
    public enum PType
    {
        Empty,
        Normal,
        Count,
        Bomb,
        Freeze
    };

    //Struttura che associa un tipo di pezzo al suo prefab
    [System.Serializable]
    public struct PPrefab
    {
        public PType type;
        public GameObject prefab;
    }

    public PPrefab[] piecePrefabs;
    public GameObject backgroundPF;
    public Dictionary<PType, GameObject> PPrefabDict;
    private GamePiece[,] pieces;
    public Game game;
    // Start is called before the first frame update
    void Start()
    {
        //riempio il Dictionary con una serie di coppie key-tipo e reelativo prefab
        PPrefabDict = new Dictionary<PType, GameObject>();
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!PPrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                PPrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        //istanzio il background
        for (int i = 0; i < X; i++)
        {
            for (int j = 0; j < Y; j++)
            {
                GameObject background = (GameObject)Instantiate(backgroundPF, GetWorldPosition(i,j), Quaternion.identity);
                background.transform.parent = transform;
            }
        }

        //gestisco la griglia come una matrice di pezzi
        pieces = new GamePiece[X, Y];
        for (int i = 0; i < X; i++)
        {
            for (int j = 0; j < Y; j++)
            {
                SpawnNewPiece(i, j, PType.Empty);
            }
        }
        StartCoroutine(Fill());
        //if (CheckGameOver() == false)
        //    game.GameEnd();
    }

    //Uso una coroutine per lo spawn animato
    public IEnumerator Fill()
    {
        bool refillNecessary = true;
        while (refillNecessary)
        {
            yield return new WaitForSeconds(fillTime);
            while (FillStep())
            {
                yield return new WaitForSeconds(fillTime);
            }
            refillNecessary = ClearAllValidMatches();
        }
    }

    //Fillstep verifica, partendo dalla penultima riga se ho un pezzo posto
    //sopra uno spazio vuoto. In caso affermativo distruggo il pezzo vuoto,
    //muovo quello "valido nella posizione inferiore" e spawno un nuovo pezzo vuoto nella
    //posizione lasciata libera per riempirlo successivamente
    public bool FillStep()
    {
        bool movedPiece = false;

        for (int j = Y - 2; j >= 0; j--)
        {
            for (int i = 0; i < X; i++)
            {
                GamePiece piece = pieces[i, j];
                if (piece.IsMovable())
                {
                    GamePiece pieceBelow = pieces[i, j + 1];
                    if (pieceBelow.Type == PType.Empty)
                    {
                        Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(i, j + 1, fillTime);
                        pieces[i, j + 1] = piece;
                        SpawnNewPiece(i, j, PType.Empty);
                        movedPiece = true;
                    }
                }
            }
        }

        //in questa sezione avviene l'effettivo spawn iniziale a partire da sopra la prima riga della
        //griglia. Il prefab viene istanziato e il pezzo effettivamente inizializzato con la posizione e il tipo
        //e spostato nella prima riga (il pezzo sottostante sarà sicuramente vuoto). Il colore viene settato casualmente
        //spawnata la prima riga si torna al for superiore per spostarla più in basso e di nuovo qui per spawnare la
        //successiva. FillStep continua a essere richiamato finche movedPiece è true, ovvero sicuramente finché ci saranno 
        //pezzi "vuoti" da sostituire
        for (int i = 0; i < X; i++)
        {
            GamePiece pieceBelow = pieces[i, 0];
            if (pieceBelow.Type == PType.Empty)
            {
                GameObject newPiece = (GameObject)Instantiate(PPrefabDict[PType.Normal], GetWorldPosition(i, -1), Quaternion.identity);
                newPiece.transform.parent = transform;
                pieces[i, 0] = newPiece.GetComponent<GamePiece>();
                pieces[i, 0].Initialize(i, -1, this, PType.Normal);
                pieces[i, 0].MovableComponent.Move(i, 0, fillTime);
                pieces[i, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[i, 0].ColorComponent.NumColors));
                movedPiece = true;
            }
        }
        return movedPiece;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(transform.position.x - X*0.235f + x * 0.54f, transform.position.y + Y*0.235f - y * 0.54f);
    }

    //SpawnNewPiece ricava il prefab dal dictionary e calcola la posizione in cui Istanziare il pezzo con 
    //GetWorldPosition
    public GamePiece SpawnNewPiece(int x, int y, PType type)
    {
        GameObject newPiece = (GameObject)Instantiate(PPrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;
        pieces[x, y] = newPiece.GetComponent<GamePiece>();
        pieces[x, y].Initialize(x, y, this, type);
        return pieces[x, y];
    }

    //funzione di utilità per verificare l'adiacenza di due pezzi
    public bool IsAdjacent(GamePiece piece1, GamePiece piece2)
    {
        if ((piece1.X == piece2.X && Mathf.Abs(piece1.Y - piece2.Y) == 1) || (piece1.Y == piece2.Y && Mathf.Abs(piece1.X - piece2.X) == 1))
            return true;
        else
            return false;
    }

    //swap dei pezzi selezionati (il primo pezzo toccato e il secondo su cui è stato posto il dito o il mouse
    //prima di rilasciare il controllo. Scambio i pezzi nella matrice e con GetMatch verifico se ottengo delle combo
    public void SwapPieces(GamePiece piece1, GamePiece piece2)
    {
        if (gameOver)
            return;

        if(piece1.IsMovable() && piece2.IsMovable())
        {
            pieces[piece1.X, piece1.Y] = piece2;
            pieces[piece2.X, piece2.Y] = piece1;

            //Se una combinazione è possibile muovo i pezzi e con ClearAllValidMatches elimino
            //tutti i pezzi che formano una combinazioni. Riempio i pezzi vuoti con la fill che a sua volta
            //richiama ClearAllValidMatches per eliminare eventuali nuove combinazioni formtesi dopo il
            //refill
            //CheckGameOver verifica se ci sono altre possibili combinazioni, altrimenti restituisce GameOver.
            if (GetMatch(piece1, piece2.X, piece2.Y) != null || GetMatch(piece2, piece1.X, piece1.Y) != null)
            {
                int piece1X = piece1.X;
                int piece1Y = piece1.Y;

                piece1.MovableComponent.Move(piece2.X, piece2.Y, fillTime);
                piece2.MovableComponent.Move(piece1X, piece1Y, fillTime);

                ClearAllValidMatches();

                //pressedPiece = null;
                //enteredPiece = null;

                StartCoroutine(Fill());

                if (CheckGameOver() == false)
                    game.GameEnd();
            }
            else
            {
                pieces[piece1.X, piece1.Y] = piece1;
                pieces[piece2.X, piece2.Y] = piece2;
            }
        }
    }

    public void PressPiece(GamePiece piece)
    {
        pressedPiece = piece;
    }

    public void EnterPiece(GamePiece piece)
    {
        enteredPiece = piece;
    }

    //Se i pezzi toccati sono adiacenti tento di fare lo swap
    public void ReleasePiece()
    {
        if (IsAdjacent(pressedPiece, enteredPiece))
            SwapPieces(pressedPiece, enteredPiece);
    }

    //GetMatch inizia verificando eventuali match in orizzontale. dir uguale a 0 o 1 scorre le posizioni
    //adiacenti verso sinistra e destra rispettivamente per trovare pezzi dello stesso colore o di tipo speciale
    //esegue poi lo stesso in verticale e restituisce i pezzi trovati in un array.
    public List<GamePiece> GetMatch(GamePiece piece, int landingX, int landingY)
    {
        if (piece.IsColored())
        {
            ColorPiece.ColorType color = piece.ColorComponent.Color;
            List<GamePiece> horzPieces = new List<GamePiece>();
            List<GamePiece> vertPieces = new List<GamePiece>();
            List<GamePiece> matchingPieces = new List<GamePiece>();

            //////////////////////////
            //sezione di codice da cui si passa se il pezzo swappato è un pezzo speciale (combinabile con tutti i colori)
            //per tutti i possibili colori, uno alla volta, valuta la possibilità di fare delle combinazioni
            if (color == ColorPiece.ColorType.Any)
            {
                for (int c = 0; c < 7; c++)
                {
                    horzPieces.Add(piece);
                    for (int dir = 0; dir <= 1; dir++)
                        for (int xOffset = 1; xOffset < X; xOffset++)
                        {
                            int x;
                            if (dir == 0)
                                x = landingX - xOffset;
                            else
                                x = landingX + xOffset;

                            if (x < 0 || x >= X)
                                break;

                            if (pieces[x, landingY].IsColored() && (pieces[x, landingY].ColorComponent.Color == (ColorPiece.ColorType)c || pieces[x, landingY].ColorComponent.Color == ColorPiece.ColorType.Any))
                                horzPieces.Add(pieces[x, landingY]);
                            else
                                break;
                        }

                    if (horzPieces.Count >= 3)
                        for (int i = 0; i < horzPieces.Count; i++)
                            matchingPieces.Add(horzPieces[i]);

                    //se ho fatto un match orizzontale scorro l'arrai dei match orizzantili e verifico se qualunque 
                    //dei tre pezzi forma un altro match in verticale 
                    if (horzPieces.Count >= 3)
                        for (int i = 0; i < horzPieces.Count; i++)
                            for (int dir = 0; dir <= 1; dir++)
                                for (int yOffset = 1; yOffset < Y; yOffset++)
                                {
                                    int y;
                                    if (dir == 0)
                                        y = landingY - yOffset;
                                    else
                                        y = landingY + yOffset;

                                    if (y < 0 || y >= Y)
                                        break;

                                    if (pieces[horzPieces[i].X, y].IsColored() && (pieces[horzPieces[i].X, y].ColorComponent.Color == (ColorPiece.ColorType)c || pieces[horzPieces[i].X, y].ColorComponent.Color == ColorPiece.ColorType.Any))
                                        vertPieces.Add(pieces[horzPieces[i].X, y]);
                                    else
                                        break;
                                }
                    if (vertPieces.Count < 2)
                        vertPieces.Clear();
                    else
                    {
                        for (int j = 0; j < vertPieces.Count; j++)
                            matchingPieces.Add(vertPieces[j]);
                    }
                    //se ho trovato il match posso restituire l'array senza procedere ulteriormente
                    if (matchingPieces.Count >= 3)
                        return matchingPieces;

                    //ripeto la procedura in verticale. dir = 0 scorre i pezzi verso l'alto
                    //mentre dir = 1 li scorre verso il basso

                    horzPieces.Clear();
                    vertPieces.Clear();
                    vertPieces.Add(piece);
                    for (int dir = 0; dir <= 1; dir++)
                        for (int yOffset = 1; yOffset < Y; yOffset++)
                        {
                            int y;
                            if (dir == 0)
                                y = landingY - yOffset;
                            else
                                y = landingY + yOffset;

                            if (y < 0 || y >= Y)
                                break;

                            if (pieces[landingX, y].IsColored() && (pieces[landingX, y].ColorComponent.Color == (ColorPiece.ColorType)c || pieces[landingX, y].ColorComponent.Color == ColorPiece.ColorType.Any))
                                vertPieces.Add(pieces[landingX, y]);
                            else
                                break;
                        }

                    if (vertPieces.Count >= 3)
                        for (int i = 0; i < vertPieces.Count; i++)
                            matchingPieces.Add(vertPieces[i]);
                    //anche qui verifico la possibilità di altri match orizzontali se ne ho fatto uno in verticale
                    //scorrendo l'array trovato
                    if (vertPieces.Count >= 3)
                        for (int i = 0; i < vertPieces.Count; i++)
                            for (int dir = 0; dir <= 1; dir++)
                                for (int xOffset = 1; xOffset < X; xOffset++)
                                {
                                    int x;
                                    if (dir == 0)
                                        x = landingX - xOffset;
                                    else
                                        x = landingX + xOffset;

                                    if (x < 0 || x >= X)
                                        break;

                                    if (pieces[x, vertPieces[i].Y].IsColored() && (pieces[x, vertPieces[i].Y].ColorComponent.Color == (ColorPiece.ColorType)c || pieces[x, vertPieces[i].Y].ColorComponent.Color == ColorPiece.ColorType.Any))
                                        horzPieces.Add(pieces[x, vertPieces[i].Y]);
                                    else
                                        break;
                                }
                    if (horzPieces.Count < 2)
                        horzPieces.Clear();
                    else
                    {
                        for (int j = 0; j < horzPieces.Count; j++)
                            matchingPieces.Add(horzPieces[j]);
                        //break;
                    }
                    if (matchingPieces.Count >= 3)
                        return matchingPieces;
                }
            }
            //fine sezione dei match per pezzi speciali
            //////////////////////////
            

            //sezione da cui passo se tra i due pezzi swappati non ce n'è uno speciale
            //scorro prima i pezzi in orizzontale. Uso dir per definire se sto andando verso sinistra (0)
            //o verso destra (1)
            
                horzPieces.Add(piece);
                for (int dir = 0; dir <= 1; dir++)
                    for (int xOffset = 1; xOffset < X; xOffset++)
                    {
                        int x;
                        if (dir == 0)
                            x = landingX - xOffset;
                        else
                            x = landingX + xOffset;

                        if (x < 0 || x >= X)
                            break;

                        if (pieces[x, landingY].IsColored() && (pieces[x, landingY].ColorComponent.Color == color || pieces[x, landingY].ColorComponent.Color == ColorPiece.ColorType.Any))
                            horzPieces.Add(pieces[x, landingY]);
                        else
                            break;
                    }

                if (horzPieces.Count >= 3)
                    for (int i = 0; i < horzPieces.Count; i++)
                        matchingPieces.Add(horzPieces[i]);

            //se ho fatto un match orizzontale scorro l'array dei match orizzantali e verifico se qualunque 
            //dei tre pezzi forma un altro match in verticale 
            if (horzPieces.Count >= 3)
                    for (int i = 0; i < horzPieces.Count; i++)
                        for (int dir = 0; dir <= 1; dir++)
                            for (int yOffset = 1; yOffset < Y; yOffset++)
                            {
                                int y;
                                if (dir == 0)
                                    y = landingY - yOffset;
                                else
                                    y = landingY + yOffset;

                                if (y < 0 || y >= Y)
                                    break;

                                if (pieces[horzPieces[i].X, y].IsColored() && (pieces[horzPieces[i].X, y].ColorComponent.Color == color || pieces[horzPieces[i].X, y].ColorComponent.Color == ColorPiece.ColorType.Any))
                                    vertPieces.Add(pieces[horzPieces[i].X, y]);
                                else
                                    break;
                            }
                if (vertPieces.Count < 2)
                    vertPieces.Clear();
                else
                {
                    for (int j = 0; j < vertPieces.Count; j++)
                        matchingPieces.Add(vertPieces[j]);
                }

                if (matchingPieces.Count >= 3)
                    return matchingPieces;

                //ripeto la procedura in verticale. dir = 0 scorre i pezzi verso l'alto
                //mentre dir = 1 li scorre verso il basso

                horzPieces.Clear();
                vertPieces.Clear();
                vertPieces.Add(piece);
                for (int dir = 0; dir <= 1; dir++)
                    for (int yOffset = 1; yOffset < Y; yOffset++)
                    {
                        int y;
                        if (dir == 0)
                            y = landingY - yOffset;
                        else
                            y = landingY + yOffset;

                        if (y < 0 || y >= Y)
                            break;

                        if (pieces[landingX, y].IsColored() && (pieces[landingX, y].ColorComponent.Color == color || pieces[landingX, y].ColorComponent.Color == ColorPiece.ColorType.Any))
                            vertPieces.Add(pieces[landingX, y]);
                        else
                            break;
                    }

                if (vertPieces.Count >= 3)
                    for (int i = 0; i < vertPieces.Count; i++)
                        matchingPieces.Add(vertPieces[i]);
                //ricerca di eventuali match orizzontali per i pezzi che ne hanno
                //formato uno in verticale
                if (vertPieces.Count >= 3)
                    for (int i = 0; i < vertPieces.Count; i++)
                        for (int dir = 0; dir <= 1; dir++)
                            for (int xOffset = 1; xOffset < X; xOffset++)
                            {
                                int x;
                                if (dir == 0)
                                    x = landingX - xOffset;
                                else
                                    x = landingX + xOffset;

                                if (x < 0 || x >= X)
                                    break;

                                if (pieces[x, vertPieces[i].Y].IsColored() && (pieces[x, vertPieces[i].Y].ColorComponent.Color == color || pieces[x, vertPieces[i].Y].ColorComponent.Color == ColorPiece.ColorType.Any))
                                    horzPieces.Add(pieces[x, vertPieces[i].Y]);
                                else
                                    break;
                            }
                if (horzPieces.Count < 2)
                    horzPieces.Clear();
                else
                {
                    for (int j = 0; j < horzPieces.Count; j++)
                        matchingPieces.Add(horzPieces[j]);
                }
            
            if (matchingPieces.Count >= 3)
                return matchingPieces;
        }
        //se nessuna ricerca ha restituito match 
        return null;
    }

    //ClearAllValidMatches scorre la matrice dopo uno swap e cerca i match presenti sulla griglia
    //Se il match è non nullo eseguo azioni diverse a seconda del numero di pezzi
    //della combinazione
    public bool ClearAllValidMatches()
    {
        bool refillNecessary = false;
        for (int y = 0; y < Y; y++)
            for (int x = 0; x < X; x++)
                if (pieces[x, y].IsClearable())
                {
                    List<GamePiece> match = GetMatch(pieces[x, y], x, y);
                    if(match!= null)
                    {
                        //Nell'eventualità di aver combinato 5 pezzi seleziono un pezzo casuale 
                        //Ne inizializzo il tipo e la posizione con valori che sostituirò 
                        //nei blocchi annidati
                        PType SpecialPType = PType.Count;
                        GamePiece randomPiece = match[Random.Range(0, match.Count)]; //
                        int specialPieceX = randomPiece.X;
                        int specialPieceY = randomPiece.Y;

                        //setto il tipo a bomba o freeze a seconda del valore della variabile powerup
                        if (match.Count >= 5 && PowerUpChoice.powerup == 0)
                        {
                            SpecialPType = PType.Bomb;
                        }
                        else if (match.Count >= 5 && PowerUpChoice.powerup == 1)
                        {
                            SpecialPType = PType.Freeze;
                        }
                        //Elimino i singoli
                        for (int i = 0; i < match.Count; i++)
                            if (ClearPiece(match[i].X, match[i].Y)) 
                            {
                                refillNecessary = true;
                                //setto la posizione dell'eventuale pezzo specisle da spawnare
                                if (match[i] == pressedPiece || match[i] == enteredPiece) 
                                {
                                    specialPieceX = match[i].X;
                                    specialPieceY = match[i].Y;
                                }
                            }
                        //se ho settato il tipo di un pezzo speciale lo devo spawnare e settarne il colore
                        if (SpecialPType != PType.Count)
                        {
                            Destroy(pieces[specialPieceX, specialPieceY]);
                            GamePiece newPiece = SpawnNewPiece(specialPieceX, specialPieceY, SpecialPType);

                             if ((SpecialPType == PType.Bomb || SpecialPType == PType.Freeze) && newPiece.IsColored())
                                newPiece.ColorComponent.SetColor(ColorPiece.ColorType.Any);
                        }
                    }
                }
        return refillNecessary;
    }
    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].IsClearable() && !pieces[x, y].ClearableComponent.IsBeingCleared)
        {
            pieces[x, y].ClearableComponent.Clear();
            SpawnNewPiece(x, y, PType.Empty);

            return true;
        }
        return false;
    }

    //Se il pezzo è una bomba elimino tutti i pezzi circostanti nel raggio di due caselle
    public void ClearBomb(int x, int y)
    {
        for (int i = x - 2; i <= x + 2; i++)
            for (int j = y - 2; j <= y + 2; j++)
                if (i != -2 && i != -1 && i != 8 && i != 9 && j != -1 && j != 8 && j != -2 && j != 9)
                    ClearPiece(i, j);
    }

    //Se il pezzo è un Freeze lancio una coroutine per fermare il timer per 5 secondi
    public void ClearFreeze()
    {
        StartCoroutine(EnableTimedBonus());
    }

    //stopTimer è una variabile statica che viene usata dallo script Game per sapere quando fermare il timer
    IEnumerator EnableTimedBonus()
    {
        stopTimer = true;
        yield return new WaitForSeconds(5.0f);
        stopTimer = false;
    }

    public void GameOver()
    {
        gameOver = true;
    }

    //checkGameOver scambia due pezzi adiacenti nella matrice senza fare uno swap grafico
    //verifica se ci sono ancora match possibili, se non ne trova restituisce false ed è gameOver
    //scambio sempre col pezzo alla mia destra (se sono in posizione 1 valuto anche lo scambio 
    //a sinistra. Eseguo lo stesso scambiando pezzi adiacenti in verticale.
    private bool CheckGameOver()
    {
        for (int i = 1; i < X-1; i++)
        {
            for (int j = 0; j < Y; j++)
            {
                GamePiece appPieceRight = pieces[i, j];
                pieces[i, j] = pieces[i + 1, j];
                pieces[i + 1, j] = appPieceRight;
                GamePiece appPieceRight2;
                if (GetMatch(pieces[i, j], i, j) != null || GetMatch(pieces[i + 1, j], i +1 , j) != null)
                {
                    appPieceRight2 = pieces[i, j];
                    pieces[i, j] = pieces[i + 1, j];
                    pieces[i + 1, j] = appPieceRight2;
                    return true;
                }
                appPieceRight2 = pieces[i, j];
                pieces[i, j] = pieces[i + 1, j];
                pieces[i + 1, j] = appPieceRight2;

                if (i == 1)
                {
                    GamePiece appPieceLeft = pieces[i, j];
                    pieces[i, j] = pieces[i - 1, j];
                    pieces[i - 1, j] = appPieceLeft;
                    GamePiece appPieceLeft2;
                    if (GetMatch(pieces[i, j], i, j) != null || GetMatch(pieces[i - 1, j], i -1 , j) != null)
                    {
                        appPieceLeft2 = pieces[i, j];
                        pieces[i, j] = pieces[i - 1, j];
                        pieces[i - 1, j] = appPieceLeft2;
                        return true;
                    }
                    appPieceLeft2 = pieces[i, j];
                    pieces[i, j] = pieces[i - 1, j];
                    pieces[i - 1, j] = appPieceLeft2;
                }
            }
        }

        for (int i = 0; i < X; i++)
        {
            for (int j = 1; j < Y-1; j++)
            {
                GamePiece appPieceRight = pieces[i, j];
                pieces[i, j] = pieces[i, j + 1];
                pieces[i , j + 1] = appPieceRight;
                GamePiece appPieceRight2;
                if (GetMatch(pieces[i, j], i, j) != null || GetMatch(pieces[i, j + 1], i, j + 1) != null)
                {
                    appPieceRight2 = pieces[i, j];
                    pieces[i, j] = pieces[i, j +1];
                    pieces[i, j + 1] = appPieceRight2;
                    return true;
                }
                appPieceRight2 = pieces[i, j];
                pieces[i, j] = pieces[i, j + 1];
                pieces[i, j +1] = appPieceRight2;

                if (j == 1)
                {
                    GamePiece appPieceLeft = pieces[i, j];
                    pieces[i, j] = pieces[i, j - 1];
                    pieces[i, j - 1] = appPieceLeft;
                    GamePiece appPieceLeft2;
                    if (GetMatch(pieces[i, j], i, j) != null || GetMatch(pieces[i, j - 1], i, j - 1) != null)
                    {
                        appPieceLeft2 = pieces[i, j];
                        pieces[i, j] = pieces[i, j - 1];
                        pieces[i, j - 1] = appPieceLeft2;
                        return true;
                    }
                    appPieceLeft2 = pieces[i, j];
                    pieces[i, j] = pieces[i, j - 1];
                    pieces[i, j - 1] = appPieceLeft2;
                }
            }
        }

        return false;
    }
}
