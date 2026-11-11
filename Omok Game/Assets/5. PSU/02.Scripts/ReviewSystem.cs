using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReviewSystem : MonoBehaviour
{
    [SerializeField] Transform AutoPlayingUI;
    [SerializeField] TextMeshProUGUI turnUI;
    [SerializeField] Image turnImage;
    int currIndex = -1;
    int CurrIndex
    {
        get { return currIndex; }
        set
        {
            currIndex = value;
            turnUI.text = $"{currIndex + 1}/{reviewMoves.Count}";
            turnImage.fillAmount = (float)(currIndex + 1) / reviewMoves.Count;
        }
    }
    List<Point> reviewMoves;
    GameLogic gameLogic;
    Ruler ruler;
    bool isAuto = false;

    public void Init(GameLogic gameLogic, Ruler ruler)
    {
        reviewMoves = GameManager.Instance.currentReviewData.reviewMoves;
        this.gameLogic = gameLogic;
        this.ruler = ruler;
        CurrIndex = -1;

    }
    public void OnclickNextMove()
    {
        NextMove();
        isAuto = false;
    }
    private void NextMove()
    {
        if (CurrIndex < reviewMoves.Count - 1)
        {
            CurrIndex++;
            var move = reviewMoves[CurrIndex];
            gameLogic.ChangeBoard(move.x, move.y, (Constants.PlayerType)((CurrIndex % 2) + 1));
            ruler.StartRenjuRules();
        }

    }
    public void PrevMove()
    {
        if (CurrIndex >= 0)
        {
            var move = reviewMoves[CurrIndex];
            gameLogic.ChangeBoard(move.x, move.y, Constants.PlayerType.None);
            ruler.StartRenjuRules();
            CurrIndex--;
        }
        isAuto = false;
    }

    public void ToLastMove()
    {
        while (CurrIndex < reviewMoves.Count - 1)
        {
            CurrIndex++;
            var move = reviewMoves[CurrIndex];
            gameLogic.ChangeBoard(move.x, move.y, (Constants.PlayerType)((CurrIndex % 2) + 1));
        }
        isAuto = false;
        ruler.StartRenjuRules();
    }
    public void ToFirstMove()
    {
        while (CurrIndex >= 0)
        {
            var move = reviewMoves[CurrIndex];
            gameLogic.ChangeBoard(move.x, move.y, Constants.PlayerType.None);
            CurrIndex--;
        }
        isAuto = false;
        ruler.StartRenjuRules();
    }

    public void AutoReview()
    {
        isAuto = !isAuto;
        if (isAuto)
            StartCoroutine(AutoReviewCoroutine());

    }
    public void OnClickReturnMain()
    {
        MainManager.Instance.LoadMainSceneWithUI("Main Menu");
    }

    IEnumerator AutoReviewCoroutine()
    {
        StartCoroutine(SpinAutoRoutine());

        yield return new WaitForSeconds(1f);

        while (isAuto && CurrIndex < reviewMoves.Count - 1)
        {
            NextMove();
            StartCoroutine(SpinAutoRoutine());
            yield return new WaitForSeconds(1f);
        }
        isAuto = false;
    }
    IEnumerator SpinAutoRoutine()
    {
        var wait = Util.GetWaitForSeconds(1 / 180);
        for (int i = 0; i < 180; i++)
        {
            AutoPlayingUI.Rotate(new Vector3(0, 0, 1));
            yield return wait;
        }

    }
}