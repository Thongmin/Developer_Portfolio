using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviewButton : MonoBehaviour
{
    public ReviewData myData;

    private void OnEnable()
    {
        transform.GetComponent<Button>().onClick.AddListener(OnReviewBtClk);
    }

    public void TextSet()
    {
        TextMeshProUGUI[] texts = transform.GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var text in texts)
        {
            GameLogger.Log(text.name);
        }

        texts[0].text = "날짜 : " + myData.reviewKey;
        texts[1].text = "모드 : " + myData.gameState.ToString();

        switch (myData.gameState)
        {
            // ai
            case PlayState.SinglePlay:
                texts[2].text = myData.winnerPlayer == PlayerState.AI ? "결과 : 패" : "결과 : 승";
                texts[3].text = "돌 : " + myData.playerColor;
                break;
            // 로컬
            case PlayState.DualPlay:
                texts[2].text = myData.winnerPlayer == PlayerState.PlayerA ? "흑 승" : "백 승";
                texts[3].text = "돌 : -";
                break;
            // 멀티
            case PlayState.MultiPlay:
                texts[2].text = myData.winnerPlayer == PlayerState.MultiPlayer ? "결과 : 패" : "결과 : 승";
                texts[3].text = "돌 : -";
                break;
        }
    }

    void OnReviewBtClk()
    {
        GameManager.Instance.gameState = PlayState.Review;

        GameManager.Instance.currentReviewData.gameState = myData.gameState;
        GameManager.Instance.currentReviewData.reviewKey = myData.reviewKey;
        GameManager.Instance.currentReviewData.reviewMoves = new System.Collections.Generic.List<Point>();
        for (int i = 0; i < myData.reviewMoves.Count; i++) {
            GameManager.Instance.currentReviewData.reviewMoves.Add(myData.reviewMoves[i]);
        }


        MainManager.Instance.LoadGameScene();
    }
}
