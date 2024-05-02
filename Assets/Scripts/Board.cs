using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // Start is called before the first frame update

    public int width;
    public int height;

    public GameObject gbTilePrefab;
    public Gem[] gems;
    public Gem[,] allGems;

    public float gemSpeed;

    [HideInInspector]
    public MatchFinder matchFind;

    public enum BoardState { wait, move}
    public BoardState currentState = BoardState.move;

    public Gem bomb;
    public float bombChance = 2f;

    public RoundManager roundMan;

    private float bonusMulti;
    public float bonusAmount = 0.5f;

    

    private void Awake()
    {
        matchFind = FindObjectOfType<MatchFinder>();
        roundMan = FindObjectOfType<RoundManager>();
    }
    void Start()
    {
        allGems = new Gem[width,height];

        Setup();

        
    }

    private void Update()
    {
       // matchFind.FindAllMatches(); 
    }
    private void Setup()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                GameObject bgTile = Instantiate(gbTilePrefab, pos, Quaternion.identity);
                bgTile.transform.parent = transform;
                bgTile.name = "BG Tile - " + x + ", " + y;

                int gemToUse = Random.Range(0, gems.Length);

                int iteration = 0;

                while(MatcheaAt(new Vector2Int(x, y), gems[gemToUse]) && iteration < 100)
                {
                    gemToUse = Random.Range(0, gems.Length);
                    iteration++;
                }

                SpawnGem(new Vector2Int(x, y), gems[gemToUse]);
            }
        }
    }

    private void SpawnGem(Vector2Int pos, Gem gemToSpawn)
    {
        if(Random.Range(0f,100f) < bombChance)
        {
            gemToSpawn = bomb;
        }

        Gem gemc = Instantiate(gemToSpawn, new Vector3(pos.x, pos.y + height, 0f), Quaternion.identity);
        gemc.transform.parent = transform;
        gemc.name = "Gem - " + pos.x + ", " + pos.y;

        allGems[pos.x, pos.y] = gemc;

        gemc.SetupGem(pos, this);
    }

    bool MatcheaAt(Vector2Int posToCheck, Gem gemToCheck)
    {
        if(posToCheck.x > 1)
        {
            if(allGems[posToCheck.x -1, posToCheck.y].type == gemToCheck.type 
                && allGems[posToCheck.x - 2, posToCheck.y].type == gemToCheck.type)
            {
                return true;
            }
        }

        if (posToCheck.y > 1)
        {
            if (allGems[posToCheck.x, posToCheck.y - 1].type == gemToCheck.type
                && allGems[posToCheck.x, posToCheck.y - 2].type == gemToCheck.type)
            {
                return true;
            }
        }

        return false;
    }


    private void DestroyMatchedGemAt(Vector2Int pos)
    {
        if(allGems[pos.x, pos.y] != null)
        {
            if(allGems[pos.x, pos.y].isMatched)
            {
                Instantiate(allGems[pos.x,pos.y].destroyEffect, new Vector2(pos.x, pos.y), Quaternion.identity);

                Destroy(allGems[pos.x, pos.y].gameObject);
                allGems[pos.x, pos.y] = null;
            }
        }
    }

    public void DestroyMatches()
    {
       for(int i = 0; i < matchFind.currentMatches.Count; i++)
        {
            if(matchFind.currentMatches[i] != null)
            {
                ScoreCheck(matchFind.currentMatches[i]);

                DestroyMatchedGemAt(matchFind.currentMatches[i].posIndex);
            }
        }

        StartCoroutine(DecreaseRowCo());
    }


    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(0.2f);

        int nullCounter = 0;


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                if(allGems[x,y] == null)
                {
                    nullCounter++;

                }
                else if(nullCounter > 0)
                {
                    allGems[x, y].posIndex.y -= nullCounter;
                    allGems[x,y - nullCounter] = allGems[x, y];
                    allGems[x, y] = null;
                }

            }
            nullCounter = 0;
        }

        StartCoroutine(FillBoardCo());

    }

    private IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(0.4f);

        RefilBoard();
        yield return new WaitForSeconds(0.4f);

        matchFind.FindAllMatches();

        if(matchFind.currentMatches.Count > 0)
        {
            bonusMulti++;
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
            currentState = BoardState.move;

            bonusMulti = 0;
        }

    }

    private void RefilBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    int gemToUse = Random.Range(0, gems.Length);

                    SpawnGem(new Vector2Int(x, y), gems[gemToUse]);
                }
            }
        }

     //   CheckMisplasedGems();
    }

    private void CheckMisplasedGems()
    {
        List<Gem> foundGems = new List<Gem>();

        foundGems.AddRange(FindObjectsOfType<Gem>());


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foundGems.Contains(allGems[x, y]))
                {
                    foundGems.Remove(allGems[x, y]);
                }
            }
        }

        foreach(Gem g in foundGems)
        {
            Destroy(g.gameObject);
        }
    }

    public void ScoreCheck(Gem gemToCheck)
    {
        roundMan.currentScore += gemToCheck.scoreValue;

        if(bonusMulti > 0)
        {
            float bonusToAdd = gemToCheck.scoreValue * bonusMulti * bonusAmount;
            roundMan.currentScore += Mathf.RoundToInt(bonusToAdd);
        }
    }

}
