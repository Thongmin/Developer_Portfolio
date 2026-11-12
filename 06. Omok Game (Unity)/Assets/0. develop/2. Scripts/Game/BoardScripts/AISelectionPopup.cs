using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AISelectionPopup : MonoBehaviour
{
    [SerializeField] TMP_InputField AILevel;
    [SerializeField] GameObject selectFirstUI;
    [SerializeField] GameObject selectSecondUI;

    int maxLevel = 3;
    private int currentAILevel = 1;
    public int CurrentAILevel
    {
        get { return currentAILevel; }
        set
        {
            if (value <= 0)
                currentAILevel = 1;
            if (value > maxLevel)
                value = maxLevel;
            currentAILevel = value;
            Debug.Log(currentAILevel.ToString());
            AILevel.text = currentAILevel.ToString();
        }
    }
    private bool isFirst = true;
    private Action<bool, int> DoDecide;


    public void OpenAISeletion(Action<bool, int> toDo)
    {
        gameObject.SetActive(true);
        DoDecide += toDo;
    }
    public void OnClickPlusButton()
    {
        if (CurrentAILevel < maxLevel)
            CurrentAILevel++;
    }
    public void OnClickMinusButton()
    {
        if (CurrentAILevel > 1)
            CurrentAILevel--;
    }
    public void LevelChange()
    {
        if (AILevel.text == "" || CurrentAILevel == int.Parse(AILevel.text)) return;
        int num = int.Parse(AILevel.text);
        CurrentAILevel = num;
    }

    public void SelectFirst()
    {
        selectFirstUI.SetActive(true);
        selectSecondUI.SetActive(false);
        isFirst = true;
    }
    public void SelectSecond()
    {
        selectFirstUI.SetActive(false);
        selectSecondUI.SetActive(true);
        isFirst = false;
    }


    public void OnClickDecideButton()
    {
        DoDecide?.Invoke(isFirst, currentAILevel);
        gameObject.SetActive(false);
    }
}
