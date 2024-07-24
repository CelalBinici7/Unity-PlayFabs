using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AOT;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using PlayFab.ExperimentationModels;
using TMPro;
using Unity.VisualScripting;
using System;


public class PlayFabManager : MonoBehaviour
{
    public uiscript uis;

    public TMP_Text welcomeText;

    public characterScript[] characterBoxes;

    public List<character> characters = new List<character>();

    [Header("Inputfields")]
    public TMP_InputField email;
    public TMP_InputField Password;
    public TMP_Text logeText;

    public TMP_Text executeText;
  
    public TMP_InputField input;
    [Header("Feedback")]
    public TMP_InputField topicInput;
    public TMP_InputField messageInput;
    [Header("Currency")]
    public TMP_Text coinsValuerText;
    public TMP_Text rubiesValuerText;
    public TMP_Text energyValuerText;
    public TMP_Text energyRechargeTimerText;
   
    [Header("LeaderBoard UI")]
    public GameObject nameWindow;
    public GameObject leaderdboardWindow;
    public TMP_InputField nameInput;
    public Transform rowParent;
    public GameObject rowPrefab;

    string loggedInPlayfabId;

    float secondsLeftToFreshEnergy = 1;

    string playerName;
    void Start()
    {
        login();
      
      
    }
    private void Update()
    {
        secondsLeftToFreshEnergy -= Time.deltaTime;
        TimeSpan time = TimeSpan.FromSeconds(secondsLeftToFreshEnergy);
        energyRechargeTimerText.text = time.ToString("mm':'ss");
        if (secondsLeftToFreshEnergy < 0)
        {
            getVirtualCurrencies();
        }
       
    }
    //Cloud Script
    public void ExecuteButton()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "Hello",
            FunctionParameter = new
            {
                name = input.text
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request,OnExecuteSucces, OnError);
    }
    public void OnExecuteSucces(ExecuteCloudScriptResult result)
    {
       executeText.text = result.FunctionResult.ToString();
    }

    //Summon nearby players in the leaderboard
    public void GetLeaderBoardAroundPLayer()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "leaderBoardScore",
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderBoardAroundPLayer, OnError);
    }
    public void OnLeaderBoardAroundPLayer( GetLeaderboardAroundPlayerResult result)
    {
        foreach (Transform item in rowParent)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in result.Leaderboard)
        {
            GameObject row = Instantiate(rowPrefab, rowParent.transform);
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = (item.Position + 1).ToString();
            texts[1].text = item.PlayFabId.ToString();//item.DisplayName.ToString();
            texts[2].text = item.StatValue.ToString();

            if (item.PlayFabId==loggedInPlayfabId)
            {
                texts[0].color = Color.cyan;
                texts[1].color = Color.cyan;
                texts[2].color = Color.cyan;
            }
        }
    }
    //CheaterFound cloud 
    public void ValidateScore(int score)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "validateScore",
            FunctionParameter = new
            {
                score = score
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request,OnExecuteSucces, OnError);
    }
    public void SendFeedback()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "sendFeedback",
            FunctionParameter = new
            {
                topic = topicInput.text,
                message = messageInput.text,
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, OnExecuteSucces, OnError);
    }
    //GetUserInventoryRequest  command calls inventory and currency values
    public void getVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),OnGetUserInventorySucces,OnError);
    }

    public void OnGetUserInventorySucces(GetUserInventoryResult result)
    {
        int coins = result.VirtualCurrency["CN"];
        coinsValuerText.text = coins.ToString();

        int rubies = result.VirtualCurrency["RB"];
        rubiesValuerText.text = coins.ToString();

        int energy = result.VirtualCurrency["EN"];
        energyValuerText.text = energy.ToString();
        secondsLeftToFreshEnergy = result.VirtualCurrencyRechargeTimes["EN"].SecondsToRecharge;
    }
    //Spend 
    public void buyItem()
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = 10,
            
        };
        PlayFabClientAPI.SubtractUserVirtualCurrency(request,OnSubtractCoinSucces, OnError);
    }

    public void OnSubtractCoinSucces(ModifyUserVirtualCurrencyResult reult)
    {
        Debug.Log("Bought item");
        getVirtualCurrencies();
    }
    //coin earning gift and can be given upon completion of the event
    public void GrantVirtualCurrency()
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = 10,
        };
        PlayFabClientAPI.AddUserVirtualCurrency(request,OnGrantVirtualCurrencySucces, OnError);
    }

    public void OnGrantVirtualCurrencySucces(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log("currency granted");
    }
  

    void login()
    {
     
        var request = new LoginWithCustomIDRequest
        {
           
            CustomId = SystemInfo.deviceUniqueIdentifier,
           
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,              
                
            }
                       
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnSucces, OnError);
    }

    public void LoginButtton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email.text,
            Password = Password.text
            
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccesLogin, OnError);
    }

    public void OnSuccesLogin(LoginResult result)
    {
       
        
        logeText.text = "Login Succesful";
    }
    //Set displayname
    public void SubmitNameButton()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nameInput.text
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
        
    }

    public void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult resullt)
    {
        nameWindow.SetActive(false);
        leaderdboardWindow.SetActive(true);
    }

    public void RegisterAndLoginButtton()
    {
        if (Password.text.Length<6)
        {
            logeText.text = "Password is too short";
            return;
        }
        var request = new RegisterPlayFabUserRequest
        {           
            Email = email.text,
            Password = Password.text,
            RequireBothUsernameAndEmail = false
           
            
            
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSucces, OnError);
    }
    public void OnRegisterSucces(RegisterPlayFabUserResult result)
    {
        logeText.text = "Register and login in";
    }
    public void ResetPasswordButtton()
    {
        var request = new SendAccountRecoveryEmailRequest { Email = email.text, TitleId = "7AE00" };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, onPasswordReset, OnError);
    }
    void onPasswordReset(SendAccountRecoveryEmailResult result  )
    {
        Debug.Log("Password reset mail send!");
    }

    void OnSucces(LoginResult loginResult)
    {
        loggedInPlayfabId = loginResult.PlayFabId;
        if (loginResult.InfoResultPayload.PlayerProfile != null)
        {
           
            playerName = loginResult.InfoResultPayload.PlayerProfile.DisplayName;
        }
        if (playerName == null)
        {
           
            nameWindow.SetActive(true);
        }
        else
        {
            leaderdboardWindow.SetActive(true);
        }
        Debug.Log("Succesful login/account create!");
        GetAppearance();
        GetTitleData();
        GetCharacters();
        getVirtualCurrencies();
    }
    void OnError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
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
        foreach (Transform item in rowParent)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in result.Leaderboard)
        {
            GameObject row=Instantiate(rowPrefab,rowParent.transform);
           TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = (item.Position+1).ToString();
            texts[1].text = item.PlayFabId.ToString();//item.DisplayName.ToString();
            texts[2].text = item.StatValue.ToString();


        }
        foreach (var item in result.Leaderboard)
        {
            
            print(item.PlayFabId);
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

    public void SaveCharacters()
    {
      
        foreach (var item in characterBoxes)
        {
            characters.Add(item.ReturnCharacter());
          
        }
     
        Debug.Log(JsonUtility.ToJson(characters));
        var request = new UpdateUserDataRequest
        {
            Data= new Dictionary<string, string> {

                {"characters",JsonUtility.ToJson(characters) }
             }
        };
        PlayFabClientAPI.UpdateUserData(request,OnDataSend,OnError);
    }

    public void GetCharacters()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnCharacterDataReceived, OnError);
    }
    public void OnCharacterDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("characters"))
        {
            List<character> list = new();
            list = JsonUtility.FromJson<List<character>>(result.Data["characters"].Value);

            for (int i = 0; i < characterBoxes.Length; i++)
            {
                characterBoxes[i].setUi(characters[i]);
            }
        }
    
    }
  

}
