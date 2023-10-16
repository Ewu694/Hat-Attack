using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Mole> hats;

    [Header("UI Objects")]
    [SerializeField] private GameObject playButton;

    private float startingTime = 30f;
    public float timeRemaining;
    private HashSet<Mole> currentHats = new HashSet<Mole>();
    private int score;
    private bool playing = false;

    public void GameOver(int type)
    {
        foreach(Mole hat in hats)
        {
            hat.StopGame();
        }
        playing = false;
        playButton.SetActive(true);
    }

    public void StartGame()
    {
        playButton.SetActive(false);//once game ends, hides all the hats
        for(int i = 0; i < hats.Count; ++i)
        {
            hats[i].Hide();
            hats[i].SetIndex(i);
        }

        currentHats.Clear();//remove any old game state
        timeRemaining = startingTime;//resets the timer
        score = 0;
        playing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(playing)
        {
            timeRemaining -= Time.deltaTime;
            if(timeRemaining <= 0)
            {
                timeRemaining = 0;
                GameOver(0);
            }
            if(currentHats.Count <= (score / 10))//every 10 hats, more hats appear
            {
                int index = Random.Range(0, hats.Count);
                if (!currentHats.Contains(hats[index]))
                {
                    currentHats.Add(hats[index]);
                    hats[index].Activate(score / 10);
                }    
            }
        }
   
    }

    public void AddScore(int hatIndex)
    {
        //to be implemented by Joshua
    }

    public void Missed(int hatIndex, bool isHat)
    {
        if(isHat)
        {
            timeRemaining -= 3;//decreases the time if a mole goes by unhit
        }
        currentHats.Remove(hats[hatIndex]);//remove the current mole from the hash set
    }
}
