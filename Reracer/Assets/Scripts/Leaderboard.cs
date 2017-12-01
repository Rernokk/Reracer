using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Leaderboard : NetworkBehaviour
{
  [SyncVar]
  public string myLeaderText, myScoreText, myMainText;
  int count = 0;

  public SyncListFloat leaderboard = new SyncListFloat();
  public SyncListInt scoreboard = new SyncListInt();

  public struct LeaderPair
  {
    public float val;
    public int score;
    public string player;

    public LeaderPair(float time, int score, string p)
    {
      val = time;
      player = p;
      this.score = score;
    }
  }

  public class LeaderPairs : SyncListStruct<LeaderPair>
  {

  }
  LeaderPairs pairs = new LeaderPairs();

  [SyncVar(hook = "WinnerUpdate")]
  public string Winner;

  // Use this for initialization
  public override void OnStartServer()
  {
    myLeaderText = GetLeaderboard();
  }

  private void Awake()
  {
    leaderboard.Callback = LeaderboardUpdate;
    scoreboard.Callback = ScoreboardUpdate;
    pairs.Callback = LeaderPairUpdate;
  }

  void LeaderboardUpdate(SyncListFloat.Operation op, int index)
  {
    myLeaderText = GetLeaderboard();
    //transform.Find("Canvas/MainLeader/Time/Text").GetComponent<Text>().text = myLeaderText;
  }

  void ScoreboardUpdate(SyncListInt.Operation op, int index)
  {
    myScoreText = GetScoreboard();
    //transform.Find("Canvas/MainLeader/Score/Text").GetComponent<Text>().text = myScoreText;
  }

  void LeaderPairUpdate(SyncListStruct<LeaderPair>.Operation op, int ind)
  {
    //myMainText = GetMainLeaderboard();
    //myMainText = SetHighScores();
    myScoreText = GetScoreboard();
    myLeaderText = GetLeaderboard();
    transform.Find("Canvas/MainLeader/Text").GetComponent<Text>().text = myMainText;
    //transform.Find("Canvas/MainLeader/Score/Text").GetComponent<Text>().text = myScoreText;
    //transform.Find("Canvas/MainLeader/Time/Text").GetComponent<Text>().text = myLeaderText;
  }

  void WinnerUpdate(string ind)
  {
    transform.Find("Canvas/Winner").GetComponent<Text>().text = ind;
    print("Changed Winner Value to " + ind);
  }

  [Command]
  public void CmdAddEntry(float t, int s, string player)
  {
    pairs.Add(new LeaderPair(t, s, player));

  }

  [Command]
  public void CmdAddScore(int t, string player)
  {
    scoreboard.Add(t);
    List<int> temp = new List<int>();
    for (int i = 0; i < scoreboard.Count; i++)
    {
      temp.Add(scoreboard[i]);
    }
    scoreboard.Clear();
    temp.Sort();
    for (int i = 0; i < temp.Count; i++)
    {
      scoreboard.Add(temp[i]);
    }

    if (scoreboard.Count == 1)
    {
      Winner = player + " wins!";
    }
  }

  public string GetLeaderboard()
  {
    string text = "- Time -\n";
    List<LeaderPair> temp = new List<LeaderPair>();
    for (int i = 0; i < pairs.Count; i++)
    {
      temp.Add(pairs[i]);
    }
    temp.Sort(delegate (LeaderPair c1, LeaderPair c2) { return c1.val.CompareTo(c2.val); });

    for (int i = 0; i < 5; i++)
    {
      if (i >= pairs.Count)
      {
        text += " - : - \n";
      }
      else
      {
        text += temp[i].val.ToString().Substring(0, 5) + " sec. | " + temp[i].player + "\n";
      }
    }
    myLeaderText = text;
    return text;
  }

  public string GetScoreboard()
  {
    string text = "- Score -\n";
    List<LeaderPair> temp = new List<LeaderPair>();
    for (int i = 0; i < pairs.Count; i++)
    {
      temp.Add(pairs[i]);
    }
    temp.Sort(delegate (LeaderPair c1, LeaderPair c2) { return -c1.score.CompareTo(c2.score); });
    for (int i = 0; i < 5; i++)
    {
      if (i >= pairs.Count)
      {
        text += " - : - \n";
      }
      else
      {
        text += temp[i].score + " pts. | " + temp[i].player + "\n";
      }
    }
    myScoreText = text;
    return text;
  }

  public string GetMainLeaderboard()
  {
    string text = "Individual Leaderboard\n";
    List<LeaderPair> temp = new List<LeaderPair>();
    for (int i = 0; i < pairs.Count; i++)
    {
      temp.Add(pairs[i]);
    }
    temp.Sort(delegate (LeaderPair c1, LeaderPair c2) { return (-c1.score.CompareTo(c2.score) != 0 ? -c1.score.CompareTo(c2.score) : c1.val.CompareTo(c2.val)); });
    for (int i = 0; i < 5; i++)
    {
      if (i >= pairs.Count)
      {
        text += "Time: - | Score: - |\n";
      }
      else
      {
        text += "Score: " + temp[i].score + " | " + "Time: " + temp[i].val.ToString().Substring(0, 5) + " | " + temp[i].player + "\n";
      }
    }
    myMainText = text;
    return text;
  }

  public void SetHighScores(string[] userData)
  {
    string text = "Individual Leaderboard\n";
    for (int i = 0; i < 10; i += 2)
    {
      if (i >= userData.Length)
      {
        text += "Time: - | Score: - \n";
      }
      else
      {
        text += "Score: " + userData[i + 1] + " | Time: " + userData[i] + "\n";
      }
    }
    myMainText = text;
    transform.Find("Canvas/MainLeader/Text").GetComponent<Text>().text = myMainText;
  }
}
