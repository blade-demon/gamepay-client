using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class CheckCode : MonoBehaviour
{
    public Text LoadingText;
    public InputField inputField;
    public GameObject GameController;
    private string recordId = "";
    private string codeText = "";

    // Use this for initialization
    void Start()
    {
        LoadingText = GameObject.Find("LoadingText").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClickButtonWithEvent()
    {
        LoadingText.text = "Code 验证中...";
        recordId = GameController.GetComponent<Done_GameController>().recordId;
        codeText = inputField.text;
        StartCoroutine(ServerCheck());
    }


    IEnumerator ServerCheck()
    {
        WWWForm form = new WWWForm();
        form.AddField("code", codeText);
        form.AddField("recordId", recordId);

        UnityWebRequest www = UnityWebRequest.Post("https://gamepay-ziweigamepoch.c9users.io/services/check", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Game failed data upload successfully!");
            string returnData = ((DownloadHandler)www.downloadHandler).text;
            string checkResult = JsonConvert.DeserializeObject<string>(returnData);
            if (checkResult == "true")
            {
                // GameObject.Find("FailedPanel").SetActive(false);
                LoadingText.text = "Code is verified.";
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {

                LoadingText.text = "Code is invalid";
                LoadingText.color = Color.red;
            }
            Debug.Log(checkResult);
        }
    }
}
