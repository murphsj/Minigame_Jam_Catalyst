using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreCounter_CATALYST : MonoBehaviour, MinigameSubscriber
{
    public List<TextMeshPro> scoreBoards;
    public int requiredDrops = 20;
    List<int> dropletScores;

    void Start()
    {
        dropletScores = new List<int> { 0, 0, 0, 0 };
        Debug.Assert(scoreBoards.Count == 4, "ScoreCounter: Scoreboard not set to a valid list of 4 TextMeshPro elements");

        foreach (TextMeshPro score in scoreBoards)
        {
            score.text = "0/" + requiredDrops;
        }
    }

    private void CheckVictory()
    {
        foreach (int scored in dropletScores)
        {
            if (scored < requiredDrops) return;
        }
        // All score requirements are met, we win
        MinigameManager.SetStateToSuccess();
    }
    
    public void AddDroplet(DropletType_CATALYST type)
    {
        dropletScores[(int)type-1]++;
        scoreBoards[(int)type - 1].text = dropletScores[(int)type - 1].ToString() + "/" + requiredDrops;
        CheckVictory();
    }

    public void OnMinigameStart()
    {
        // If the timer ends without the player reaching victory conditions,
        // they have lost
        MinigameManager.SetStateToFailure();
    }

    public void OnTimerEnd()
    {
        Debug.Log("hi");
        MinigameManager.EndGame();
    }
}
