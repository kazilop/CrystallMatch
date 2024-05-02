using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    // Start is called before the first frame update

    public float roundTime = 100f;
    private UIManager uiMan;

    private bool endingRound = false;
    private Board board;

    public int currentScore;
    public float displayScore;
    public float scoreSpeed;

    public int scoreTarget1,scoreTarget2,scoreTarget3;

    void Awake()
    {
       uiMan = FindObjectOfType<UIManager>(); 
        board = FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        if(roundTime > 0)
        {
            roundTime -= Time.deltaTime;

            if(roundTime <= 0)
            {
                roundTime = 0;

                endingRound = true;
            }
        }

        if(endingRound && board.currentState == Board.BoardState.move)
        {
            WinCheck();
            endingRound = false;
        }

        uiMan.timeText.text = roundTime.ToString("0.0");
        displayScore = Mathf.Lerp(displayScore, currentScore, scoreSpeed * Time.deltaTime);
        uiMan.scoreText.text = displayScore.ToString("0");
    }

    private void WinCheck()
    {
        uiMan.roundOverScreen.SetActive(true);

        uiMan.winScore.text = currentScore.ToString();

        if(currentScore >= scoreTarget3)
        {
            uiMan.winStars3.SetActive(true);
        }

        if (currentScore >= scoreTarget2 && currentScore < scoreTarget3)
        {
            uiMan.winStars2.SetActive(true);
        }

        if (currentScore >= scoreTarget1 && currentScore < scoreTarget2)
        {
            uiMan.winStars1.SetActive(true);
        }


    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
