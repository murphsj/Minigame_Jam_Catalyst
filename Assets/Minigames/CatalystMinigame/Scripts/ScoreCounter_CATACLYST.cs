using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreCounter_CATACLYST : MonoBehaviour
{
    public List<TextMeshPro> scoreBoards;
    public int requiredDrops = 20;
    List<int> dropletScores;

    void Start()
    {
        dropletScores = new List<int> { 0, 0, 0, 0 };

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
    
    public void AddDroplet(DropletType_CATACLYST type)
    {
        Debug.Log(dropletScores.Count);
        Debug.Log((int)type-1);
        dropletScores[(int)type-1]++;
        scoreBoards[(int)type-1].text = dropletScores[(int)type-1].ToString() + "/" + requiredDrops;
    }
}
