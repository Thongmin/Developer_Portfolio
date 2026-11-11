using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public PlayState gameState;

    public ReviewData currentReviewData = new ReviewData();

    public Sprite userProflie;

}