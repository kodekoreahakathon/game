using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChattingManager : MonoBehaviour
{
    public string characterName;
    public Transform chatParent;
    private string thread_id;

    [TextArea]
    [SerializeField] private string baseUrl = "https://port-0-girlfriendapi-lzuz68m2a0209e84.sel4.cloudtype.app";

    [TextArea]
    [SerializeField] private string serverUrl = "https://port-0-girlfriendapi-lzuz68m2a0209e84.sel4.cloudtype.app/chat";
    [TextArea]
    [SerializeField] private string createThreadUrl = "https://port-0-girlfriendapi-lzuz68m2a0209e84.sel4.cloudtype.app/create_thread";

    public GameObject incomingChatPrefab;
    public GameObject myChat;

    public TMP_InputField inputField;
    public Button button;
    public ScrollRect scrollRect;
    public RectTransform contentRect;

    [TextArea] [SerializeField] private string thread_idName;

    private TextMeshProUGUI curTextUI;
    private Coroutine curCoroutine;
    private string curMessage;
    private bool coroutine_isStarted;

    private void Start()
    {
        if (!PlayerPrefs.HasKey(thread_idName))
        {
            StartCoroutine(GetThread());
        }
        else
        {
            thread_id = PlayerPrefs.GetString(thread_idName);
            print(thread_id);
            StartCoroutine(GetMessages()); // 메시지를 가져옵니다.
        }

        button.onClick.AddListener(() => SendMessage(inputField.text));
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
            string jsonResponse = request.downloadHandler.text;
            ThreadResponse response = JsonUtility.FromJson<ThreadResponse>(jsonResponse);

            thread_id = response.thread_id;
            PlayerPrefs.SetString("thread_id", response.thread_id);
            Debug.Log("Thread ID: " + thread_id);

            // Get messages after creating the thread
            StartCoroutine(GetMessages());
        }
    }

    IEnumerator GetMessages()
    {
        string url = baseUrl + "/get_message/" + thread_id;

        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            // Use JsonUtility to deserialize JSON array
            ChatMessageListWrapper wrapper = JsonUtility.FromJson<ChatMessageListWrapper>("{\"messages\":" + jsonResponse + "}");
            List<ChatMessage> messages = wrapper.messages;
            messages.Reverse();
            foreach (ChatMessage message in messages)
            {
                print($"{message.role} : {message.content}");
                if (message.role == "user")
                {
                    MyChat text = Instantiate(myChat, chatParent).GetComponent<MyChat>();
                    text.message.text = FixedText(message.content);
                }
                else if (message.role == "assistant")
                {
                    IncomingChat text = Instantiate(incomingChatPrefab, chatParent).GetComponent<IncomingChat>();
                    text.characterName.text = characterName;
                    text.message.text = FixedText(message.content);
                }
            }

            StartCoroutine(FixedToBottom());
        }
    }


    IEnumerator SendMessageToChat(string messageContent)
    {
        ForceFixed();
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
            curTextUI = text.message;
            curMessage = response.msg;
            curCoroutine = text.StartCoroutine(TextGen(response.msg, text.message));
            //text.message.text = FixedText(response.msg);

            inputField.interactable = true;
            button.interactable = true;

            StartCoroutine(FixedToBottom());
        }
    }

    public void SendMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }
        StartCoroutine(SendMessageToChat(message));

        MyChat m = Instantiate(myChat, chatParent).GetComponent<MyChat>();

        m.message.text = FixedText(message);
        Canvas.ForceUpdateCanvases();
        inputField.text = "";
        inputField.interactable = false;
        button.interactable = false;

        StartCoroutine(FixedToBottom());
    }

    private IEnumerator FixedToBottom()
    {
        yield return new WaitForSeconds(0.1f);
        print(scrollRect.verticalNormalizedPosition);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Canvas.ForceUpdateCanvases();

        // 스크롤 위치를 가장 아래로 설정
        scrollRect.verticalNormalizedPosition = 0f;
        //RectTransform contentRect = chatParent.GetComponent<RectTransform>();

        //float minDelta = Mathf.Max(0, contentRect.sizeDelta.y - 800);
        //Debug.Log(minDelta);
        //contentRect.anchoredPosition = new Vector2(0, minDelta);
    }

    void ForceFixed()
    {
        if (curTextUI == null)
        {
            return;
        }
        
        if (coroutine_isStarted)
        {
            curTextUI.text = FixedText(curMessage);
            curTextUI = null;
            curMessage = "";
            coroutine_isStarted = false;
            StopCoroutine(curCoroutine);
            curCoroutine = null;
        }
    }
    
    
    private string FixedText(string text)
    {
        string cleanedMessage = text.Replace("\r\n", " ").Replace("\n", " ");
        var newMsg = new StringBuilder();
        int index = 0;

        foreach (char c in cleanedMessage)
        {
            if (index == 0 && c == ' ') continue;
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

        coroutine_isStarted = false;

        return newMsg.ToString();
    }

    IEnumerator TextGen(string message, TextMeshProUGUI text)
    {
        coroutine_isStarted = true;
        string cleanedMessage = message.Replace("\r\n", " ").Replace("\n", " ");
        var newMsg = new StringBuilder();
        int index = 0;

        foreach (char c in cleanedMessage)
        {
            if (index == 0 && c == ' ') continue;
            if (index >= 20)
            {
                newMsg.AppendLine();
                index = 0;
                if (c == ' ')
                {
                    continue;
                }
            }

            yield return new WaitForSeconds(0.1f);
            newMsg.Append(c);
            text.text = newMsg.ToString();
            StartCoroutine(FixedToBottom());
            index++;
        }
        coroutine_isStarted = false;
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

[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatMessageListWrapper
{
    public List<ChatMessage> messages;
}