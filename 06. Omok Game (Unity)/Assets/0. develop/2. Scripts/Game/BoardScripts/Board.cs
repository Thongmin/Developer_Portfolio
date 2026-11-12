using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject clickMarker;

    [SerializeField] GameObject blockPrefab;
    [SerializeField] Vector3 blockSize = new Vector3(0.44f, 0.44f, 1);
    [SerializeField] float blockSpacing = 0.45f;
    public Block[,] blocks = new Block[15, 15];
    int[] lastClicked = new int[2];
    private void Start()
    {
        blockSpacing *= transform.localScale.x;

        // 보드 초기화
        for (int row = 0; row < 15; row++)
        {
            GameObject rowCollection = new GameObject($"Row{row}");
            rowCollection.transform.parent = this.transform;
            for (int col = 0; col < 15; col++)
            {
                GameObject blockObj = Instantiate(blockPrefab, rowCollection.transform);
                float xPos = (col - 7f) * blockSpacing;
                float yPos = (row - 7) * blockSpacing;
                blockObj.transform.position = new Vector3(xPos, yPos, 0)+transform.position;
                blockObj.transform.localScale = blockSize*transform.localScale.x;
                blocks[row, col] = blockObj.GetComponent<Block>();
                blocks[row, col].InitBlock(row, col, this);
            }
        }
        if (clickMarker != null)
        {
            clickMarker.SetActive(false);
        }
    }
    public void SetLastClickMarker(int row, int col)
    {
        if (!clickMarker.activeSelf)
        {
            clickMarker.SetActive(true);
        }
        lastClicked[0] = row;
        lastClicked[1] = col;
        clickMarker.transform.position = blocks[row, col].transform.position;
    }

    /// <summary>
    /// 마지막으로 클릭한 지점으로 착수하는 기능
    /// </summary>
    /// <param name="state"></param>
    public bool DecideLastClickBlock(Constants.PlayerType state)
    {
        if (!clickMarker.activeSelf) return false;

        if (!CheckBlockState(lastClicked[0], lastClicked[1], state)) return false;

        ChangeBlockState(lastClicked[0], lastClicked[1], state);
        GameManager.Instance.currentReviewData.reviewMoves.Add(new Point(lastClicked[0], lastClicked[1]));

        clickMarker.SetActive(false);
        return true;
    }
    public void ChangeBlockState(int row, int col, Constants.PlayerType state)
    {
        blocks[row, col].State = state;
    }
    public bool CheckBlockState(int row, int col, Constants.PlayerType state)
    {
        switch (state)
        {
            case Constants.PlayerType.PlayerB:
                if (blocks[row, col].State == Constants.PlayerType.None || blocks[row, col].State == Constants.PlayerType.Cant)
                    return true;
                else
                    return false;
            default:
                if (blocks[row, col].State == Constants.PlayerType.None)
                    return true;
                else
                    return false;
        }
    }
}
