using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using Newtonsoft.Json;

public struct MachineData
{
  public string mac;
  public string gameId;
  public string gameName;
}

public struct FailedResponseData
{
  public int __v;
  public string gameId;
  public string deviceId;
  public string _id;
  public string launchTime;
  public string[] failedList;
  public bool isPaid;
  public bool isFailed;
}

public class Done_GameController : MonoBehaviour
{
  public GameObject[] hazards;
  public Vector3 spawnValues;
  public int hazardCount;
  public float spawnWait;
  public float startWait;
  public float waveWait;

  public Text scoreText;
  public Text gameOverText;
  public Button confirmButton;
  

  public GameObject failedPanel;
  public InputField codeInputField;

  private bool gameOver;
  private bool restart;
  private int score;

  // 游戏记录ID
  public string recordId;


  void Start()
  {

    failedPanel = GameObject.Find("FailedPanel");
    failedPanel.SetActive(false);
    gameOver = false;
    restart = false;

    gameOverText.text = "";
    score = 0;
    recordId = "";
    UpdateScore();
    StartCoroutine(SpawnWaves());
    // Register machine data to server.
    StartCoroutine(RegisterToServer());
  }

  void Update()
  {
    if (restart)
    {
      if (Input.GetKeyDown(KeyCode.R))
      {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
      }
    }
  }

  void Restart()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  }

  IEnumerator SpawnWaves()
  {
    yield return new WaitForSeconds(startWait);
    while (true)
    {
      for (int i = 0; i < hazardCount; i++)
      {
        GameObject hazard = hazards[UnityEngine.Random.Range(0, hazards.Length)];
        Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
        Quaternion spawnRotation = Quaternion.identity;
        Instantiate(hazard, spawnPosition, spawnRotation);
        yield return new WaitForSeconds(spawnWait);
      }
      yield return new WaitForSeconds(waveWait);

      if (gameOver)
      {
        // restartText.text = "Press 'R' for Restart";
        restart = true;
        failedPanel.SetActive(true);
        // 上传游戏失败纪录
        StartCoroutine(GameFailed());

        break;
      }
    }
  }

  public void AddScore(int newScoreValue)
  {
    score += newScoreValue;
    UpdateScore();
  }

  void UpdateScore()
  {
    scoreText.text = "Score: " + score;
  }

  public void GameOver()
  {
    gameOverText.text = "Game Over!";
    gameOver = true;
  }


  //   public string GetMachineData()
  //   {
  //     MachineData machine = new MachineData
  //     {
  //       mac = "54:52:00:93:d6:97",
  //       gameId = "1",
  //       gameName = "打飞机"
  //     };
  //     byte[] JsonData = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(machine).ToCharArray());
  //     Debug.Log(JsonData);
  //     return JsonConvert.SerializeObject(machine);
  //   }

  IEnumerator RegisterToServer()
  {
    WWWForm form = new WWWForm();
    form.AddField("mac", "54:52:00:93:d6:97");
    form.AddField("gameId", "4");
    form.AddField("gameName", "打飞机");
    form.AddField("time", DateTime.Now.ToString());

    UnityWebRequest www = UnityWebRequest.Post("https://gamepay-ziweigamepoch.c9users.io/services/login", form);
    yield return www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError)
    {
      Debug.Log(www.error);
    }
    else
    {
      // Debug.Log("Game login data upload complete!");
      string returnData = ((DownloadHandler)www.downloadHandler).text;
      recordId = JsonConvert.DeserializeObject<FailedResponseData>(returnData)._id;
      // Debug.Log(recordId);
    }
  }

  IEnumerator GameFailed()
  {
    WWWForm form = new WWWForm();
    form.AddField("mac", "54:52:00:93:d6:97");
    form.AddField("gameId", "4");
    form.AddField("_id", recordId);
    form.AddField("time", DateTime.Now.ToString());

    UnityWebRequest www = UnityWebRequest.Post("https://gamepay-ziweigamepoch.c9users.io/services/failed", form);
    yield return www.SendWebRequest();

    if (www.isNetworkError || www.isHttpError)
    {
      Debug.Log(www.error);
    }
    else
    {
      Debug.Log("Game failed data upload successfully!");
    }
  }

}