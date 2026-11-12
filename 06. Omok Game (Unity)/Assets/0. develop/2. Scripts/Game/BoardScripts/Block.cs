using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]Board _board;
    public int blockRow;
    public int blockCol;
    SpriteRenderer spriteRenderer;
    [SerializeField] Sprite player1Sprite;
    [SerializeField] Sprite player2Sprite;
    [SerializeField] Sprite cantSprite;
    [SerializeField] Collider2D myCollider;
    private Constants.PlayerType state = Constants.PlayerType.None;
    public Constants.PlayerType State
    {
        get => state;
        set
        {
            state = value;
            switch (state)
            {
                case Constants.PlayerType.None:
                    spriteRenderer.sprite = null;
                    myCollider.enabled = true;

                    break;
                case Constants.PlayerType.PlayerA:
                    spriteRenderer.sprite = player1Sprite;
                    myCollider.enabled = false;
                    break;
                case Constants.PlayerType.PlayerB:
                    spriteRenderer.sprite = player2Sprite;
                    myCollider.enabled = false;
                    break;
                case Constants.PlayerType.Cant:
                    spriteRenderer.sprite = cantSprite;
                    myCollider.enabled = true;
                    break;
            }
        }
    }
    public void InitBlock(int row, int col, Board controller)
    {
        blockRow = row;
        blockCol = col;
        _board = controller;
        spriteRenderer = GetComponent<SpriteRenderer>();
        State = Constants.PlayerType.None;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        _board.SetLastClickMarker(blockRow, blockCol);
    }
}
