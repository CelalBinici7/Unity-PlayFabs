using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFab.ExperimentationModels;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
   public uiscript uis;

    public TMP_Text welcomeText;
    void Start()
    {
        login();
      
       
    }
    


    void login()
    {
     
        var request = new LoginWithCustomIDRequest
        {
           
            CustomId = SystemInfo.deviceUniqueIdentifier,
           
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnSucces, OnError);
    }

    void OnSucces(LoginResult loginResult)
    {
       
        Debug.Log("Succesful login/account create!");
        GetAppearance();
        GetTitleData();
    }
    void OnError(PlayFabError error)
    {
        Debug.Log("Error while login/account create!");
    }

    public void SendLeaderBoard(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate {
                  StatisticName = "leaderBoardScore",
                  Value = score
                  
                   
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }
    /// <summary>
    /// PlayFabManager.SendLeaderBoard(score or )
    /// </summary>
    /// <param name="result"></param>
    public void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Succesful leadboard sent");
    }


    public void SendButton(int score)
    {
        SendLeaderBoard(score);
    }

    public void GetLeaderBoard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "leaderBoardScore",
            StartPosition = 0,
            MaxResultsCount= 10
        };
        PlayFabClientAPI.GetLeaderboard(request,OnLeaderboardGet,OnError);
    }
    public void OnLeaderboardGet(GetLeaderboardResult result)
    {
        foreach (var item in result.Leaderboard)
        {
          
            Debug.Log(item.Position + " , " + item.PlayFabId+ " , " + item.DisplayName + " , " + item.StatValue+ " , "+ item.Profile + " , " );
        }
    }

    public void GetAppearance()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnError);
    }
    public void OnDataReceived(GetUserDataResult result)
    {
        Debug.Log("Data SReceived");
        if (result.Data!= null && result.Data.ContainsKey("Hat") && result.Data.ContainsKey("Skin") && result.Data.ContainsKey("Beard"))
        {
            uis.SetAppearance(int.Parse( result.Data["Hat"].Value), int.Parse(result.Data["Skin"].Value),int.Parse( result.Data["Beard"].Value));
        }
        else
        {
            Debug.Log("Player Data not complete");
        }
    }
    public void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Data Send");
    }
    public void SaveAppearance()
    {
        var request = new UpdateUserDataRequest
        {
           Data  = new Dictionary<string, string> {

               {"Hat" ,uis.hatindex.ToString() },
               {"Skin" ,uis.skinindex.ToString() },
               {"Beard" ,uis.beardindex.ToString() }
           } 
        };
        PlayFabClientAPI.UpdateUserData(request,OnDataSend,OnError);
    }
    public void GetTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), OtTitleDataReceived, OnError);
    }
    public void OtTitleDataReceived(GetTitleDataResult result)
    {
        if (result.Data== null || result.Data.ContainsKey("Message") == false|| result.Data.ContainsKey("Multiplier") == false)
        {
            Debug.Log("No Message");
            return;
        }
        else
        {
          //  result.Data["Message"];
            welcomeText.text = result.Data["Message"].ToString();
        }
    }

}
