using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChattingManager : MonoBehaviour
{
    public string characterName;
    public Transform chatParent;
    private string thread_id;

    [TextArea]
    [SerializeField] private string serverUrl = "https://port-0-girlfriendapi-lzuz68m2a0209e84.sel4.cloudtype.app/chat";
    [TextArea]
    [SerializeField] private string createThreadUrl = "https://port-0-girlfriendapi-lzuz68m2a0209e84.sel4.cloudtype.app/create_thread";

    public GameObject incomingChatPrefab;
    public GameObject myChat;

    public TMP_InputField inputField;
    public Button button;
    
    private void Start()
    {
        //PlayerPrefs.DeleteKey("thread_id");
        if (!PlayerPrefs.HasKey("thread_id"))
        {
            StartCoroutine(GetThread());
        }
        else
        {
            thread_id = PlayerPrefs.GetString("thread_id");
        }
    }

    IEnumerator GetThread()
    {
        UnityWebRequest request = UnityWebRequest.Get(createThreadUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Parse the response
            string jsonResponse = request.downloadHandler.text;
            ThreadResponse response = JsonUtility.FromJson<ThreadResponse>(jsonResponse);

            // Store the thread ID
            thread_id = response.thread_id;
            PlayerPrefs.SetString("thread_id", response.thread_id);
            Debug.Log("Thread ID: " + thread_id);
        }
    }

    IEnumerator SendMessageToChat(string messageContent)
    {
        MessageBody messageBody = new MessageBody();
        messageBody.thread_id = thread_id;
        messageBody.msg = messageContent;

        string jsonBody = JsonUtility.ToJson(messageBody);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            ChatResponse response = JsonUtility.FromJson<ChatResponse>(jsonResponse);

            Debug.Log("Received message: " + response.msg);
            
            IncomingChat text = Instantiate(incomingChatPrefab, chatParent).GetComponent<IncomingChat>();
            text.characterName.text = characterName;
            text.message.text = FixedText(response.msg);

            inputField.interactable = true;
            button.interactable = true;
            
            StartCoroutine(FixedToBottom());
        }
    }

    public void SendMessage(string message)
    {
        StartCoroutine(SendMessageToChat(message));

        MyChat m = Instantiate(myChat, chatParent).GetComponent<MyChat>();
        
        m.message.text = FixedText(message);
        
        inputField.text = "";
        inputField.interactable = false;
        button.interactable = false;

        StartCoroutine(FixedToBottom());
    }

    private IEnumerator FixedToBottom()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
    
        // 스크롤 위치를 가장 아래로 설정
        
        yield return null;
        
        RectTransform contentRect = chatParent.GetComponent<RectTransform>();
        
        float minDelta = Mathf.Max(0, contentRect.sizeDelta.y - 800);
        Debug.Log(minDelta);
        contentRect.anchoredPosition = new Vector2(0, minDelta);
    }

    private string FixedText(string text)
    {
        string cleanedMessage = text.Replace("\r\n", " ").Replace("\n", " ");
        var newMsg = new StringBuilder();
        int index = 0;
        
        foreach (char c in cleanedMessage)
        {
            if(index == 0 && c == ' ') continue;
            if (index >= 20)
            {
                newMsg.AppendLine();
                index = 0;
                if (c == ' ')
                {
                    continue;
                }
            }

            newMsg.Append(c);
            index++;
        }

        return newMsg.ToString();
    }
}

[System.Serializable]
public class ThreadResponse
{
    public string thread_id;
}

[System.Serializable]
public class ChatResponse
{
    public string msg;
}

[System.Serializable]
public class MessageBody
{
    public string thread_id;
    public string msg;
}

