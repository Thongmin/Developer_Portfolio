using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardUIController : MonoBehaviour
{
    [SerializeField] GameLogic gameLogic;
    [SerializeField] Image Timer;
    [SerializeField] Image BlackPlayerTurn;
    [SerializeField] Image WhitePlayerTurn;
    [SerializeField] GameObject clickMarker;
    [SerializeField] Sprite clickMarkerPlayer1;
    [SerializeField] Sprite clickMarkerPlayer2;
    [SerializeField] GameObject AIModeSelects;
    [SerializeField] Image firstPlayerProfile;
    [SerializeField] Image secondPlayerProfile;
    [SerializeField] TextMeshProUGUI firstPlayerNickname;
    [SerializeField] TextMeshProUGUI secondPlayerNickname;

    public Sprite[] testSprites;

    bool isBlackTurn = true;

    Coroutine timerCoroutine;
    private void Start()
    {
        gameLogic.OnTurnChanged += ChangeTurnUI;
        gameLogic.OnTurnChanged += RestartTimer;
    }

    void ChangeTurnUI(bool wasFirst)
    {
        BlackPlayerTurn.color = !wasFirst ? Color.gray : Color.white;
        WhitePlayerTurn.color = wasFirst ? Color.gray : Color.white;
        if (wasFirst)
        {
            clickMarker.GetComponent<SpriteRenderer>().sprite = clickMarkerPlayer2;
        }
        else
        {
            clickMarker.GetComponent<SpriteRenderer>().sprite = clickMarkerPlayer1;
        }

    }
    public void InitProfiles(bool isFirstPlayer, string nickname, Sprite sprite = null, int imageIndex = 0)
    {
        if (isFirstPlayer)
        {
            firstPlayerNickname.text = nickname;
            if (sprite == null && imageIndex < testSprites.Length)
            {
                firstPlayerProfile.sprite = testSprites[imageIndex];
            }
            else
                firstPlayerProfile.sprite = sprite;
            firstPlayerProfile.gameObject.SetActive(true);

        }
        else
        {
            secondPlayerNickname.text = nickname;
            if (sprite == null && imageIndex < testSprites.Length)
            {
                secondPlayerProfile.sprite = testSprites[imageIndex];
            }
            else
                secondPlayerProfile.sprite = sprite;
            secondPlayerProfile.gameObject.SetActive(true);

        }
    }
    public void RestartTimer(bool wasFirst)
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }
    IEnumerator TimerCoroutine()
    {
        float time = 30f;
        WaitForSeconds wait = new WaitForSeconds(.1f);
        while (time > 0)
        {

            time -= .1f;
            Timer.fillAmount = time / 30f;

            yield return wait;
        }

        //Todo:타임 오버
        gameLogic.TimeOver();
    }
    public void StopTimer()
    {
        StopCoroutine(timerCoroutine);
    }
}
