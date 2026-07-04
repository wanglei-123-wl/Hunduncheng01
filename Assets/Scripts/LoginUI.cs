using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private InputField usernameInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Text messageText;

    [Header("Server")]
    [SerializeField] private string localServerUrl = "http://127.0.0.1:8080";
    [SerializeField] private string remoteServerUrl = "http://YOUR_SERVER_IP:8080";

    private string ServerBaseUrl => Application.isEditor ? localServerUrl : remoteServerUrl;
    private string LoginUrl => ServerBaseUrl.TrimEnd('/') + "/api/login";

    private Coroutine loginCoroutine;
    private GameObject characterSelectView;
    private string selectedCharacter = "";

    private void Awake()
    {
        ConfigureInputFields();

        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnLoginButtonClick);
        }

        SetMessage("");
    }

    private void OnDestroy()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(OnLoginButtonClick);
        }
    }

    public void SetReferences(InputField username, InputField password, Button button, Text message)
    {
        usernameInput = username;
        passwordInput = password;
        loginButton = button;
        messageText = message;
        ConfigureInputFields();
    }

    private void ConfigureInputFields()
    {
        ConfigureInputField(usernameInput, "username", false);
        ConfigureInputField(passwordInput, "password", true);
    }

    private static void ConfigureInputField(InputField input, string placeholderText, bool password)
    {
        if (input == null)
        {
            return;
        }

        input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
        input.inputType = password ? InputField.InputType.Password : InputField.InputType.Standard;
        input.lineType = InputField.LineType.SingleLine;
        input.shouldActivateOnSelect = true;
        input.caretBlinkRate = 0.85f;
        input.caretWidth = 2;
        input.customCaretColor = true;
        input.caretColor = new Color(0.08f, 0.09f, 0.11f, 1f);

        if (input.placeholder is Text placeholder)
        {
            placeholder.text = placeholderText;
            placeholder.raycastTarget = false;
        }

        if (input.textComponent != null)
        {
            input.textComponent.raycastTarget = false;
        }

        AddActivateOnPointerClick(input);
        input.ForceLabelUpdate();
    }

    private static void AddActivateOnPointerClick(InputField input)
    {
        EventTrigger trigger = input.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = input.gameObject.AddComponent<EventTrigger>();
        }

        AddTriggerEntryIfMissing(trigger, EventTriggerType.PointerClick, () => input.ActivateInputField());
        AddTriggerEntryIfMissing(trigger, EventTriggerType.Select, () => input.ActivateInputField());
    }

    private static void AddTriggerEntryIfMissing(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
    {
        for (int i = 0; i < trigger.triggers.Count; i++)
        {
            if (trigger.triggers[i].eventID == eventType)
            {
                return;
            }
        }

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    private void OnLoginButtonClick()
    {
        if (loginCoroutine != null)
        {
            return;
        }

        string username = usernameInput != null ? usernameInput.text.Trim() : "";
        string password = passwordInput != null ? passwordInput.text.Trim() : "";

        if (string.IsNullOrEmpty(username))
        {
            SetMessage("Please enter username");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetMessage("Please enter password");
            return;
        }

        loginCoroutine = StartCoroutine(LoginRequest(username, password));
    }

    private IEnumerator LoginRequest(string username, string password)
    {
        SetMessage("Logging in...");
        SetLoginButtonEnabled(false);

        LoginRequestData requestData = new LoginRequestData
        {
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(LoginUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            SetLoginButtonEnabled(true);
            loginCoroutine = null;

            if (request.result != UnityWebRequest.Result.Success)
            {
                SetMessage("Server connection failed: " + request.error);
                yield break;
            }

            LoginResponseData responseData;
            try
            {
                responseData = JsonUtility.FromJson<LoginResponseData>(request.downloadHandler.text);
            }
            catch (Exception)
            {
                SetMessage("Bad server response");
                yield break;
            }

            if (responseData != null && responseData.success)
            {
                PlayerPrefs.SetString("token", responseData.token ?? "");
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();
                SetMessage("Login success");
                ShowCharacterSelectView(username);
            }
            else
            {
                string serverMessage = responseData != null ? responseData.message : "Login failed";
                SetMessage(string.IsNullOrEmpty(serverMessage) ? "Login failed" : serverMessage);
            }
        }
    }

    private void SetLoginButtonEnabled(bool enabled)
    {
        if (loginButton != null)
        {
            loginButton.interactable = enabled;
        }
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    private void ShowCharacterSelectView(string username)
    {
        SetObjectActive(usernameInput, false);
        SetObjectActive(passwordInput, false);
        SetObjectActive(loginButton, false);
        SetMessage("");
        MoveMessageTextForCharacterSelect();

        Text titleText = transform.Find("TitleText") != null ? transform.Find("TitleText").GetComponent<Text>() : null;
        if (titleText != null)
        {
            titleText.text = "Select Character";
        }

        if (characterSelectView != null)
        {
            characterSelectView.SetActive(true);
            return;
        }

        characterSelectView = new GameObject("CharacterSelectView");
        characterSelectView.transform.SetParent(transform, false);
        RectTransform rootRect = characterSelectView.AddComponent<RectTransform>();
        Stretch(rootRect);

        Text welcomeText = CreateText("WelcomeText", characterSelectView.transform, "Welcome, " + username, 24, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.82f, 0.88f, 1f));
        SetRect(welcomeText.rectTransform, 0f, 80f, 420f, 45f);

        Text selectedText = CreateText("SelectedText", characterSelectView.transform, "No character selected", 24, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.55f));
        SetRect(selectedText.rectTransform, 0f, -85f, 430f, 42f);

        CreateCharacterButton("WarriorButton", "Warrior", -135f, 10f, new Color(0.72f, 0.22f, 0.18f, 1f), selectedText);
        CreateCharacterButton("MageButton", "Mage", 0f, 10f, new Color(0.18f, 0.38f, 0.82f, 1f), selectedText);
        CreateCharacterButton("ArcherButton", "Archer", 135f, 10f, new Color(0.18f, 0.55f, 0.28f, 1f), selectedText);

        Button enterButton = CreateButton("EnterGameButton", characterSelectView.transform, "Enter Game", new Color(0.12f, 0.45f, 0.9f, 1f));
        SetRect(enterButton.GetComponent<RectTransform>(), 0f, -145f, 300f, 56f);
        enterButton.onClick.AddListener(() => EnterGame(selectedText));
    }

    private void CreateCharacterButton(string objectName, string characterName, float x, float y, Color color, Text selectedText)
    {
        Button button = CreateButton(objectName, characterSelectView.transform, characterName, color);
        SetRect(button.GetComponent<RectTransform>(), x, y, 120f, 88f);
        button.onClick.AddListener(() =>
        {
            selectedCharacter = characterName;
            PlayerPrefs.SetString("selectedCharacter", selectedCharacter);
            PlayerPrefs.Save();
            selectedText.text = "Selected: " + selectedCharacter;
            SetMessage("");
        });
    }

    private void EnterGame(Text selectedText)
    {
        if (string.IsNullOrEmpty(selectedCharacter))
        {
            SetMessage("Please select a character");
            return;
        }

        SetMessage("Ready to enter game as " + selectedCharacter);
    }

    private void MoveMessageTextForCharacterSelect()
    {
        if (messageText == null)
        {
            return;
        }

        SetRect(messageText.rectTransform, 0f, -190f, 430f, 36f);
    }

    private static void SetObjectActive(Selectable selectable, bool active)
    {
        if (selectable != null)
        {
            selectable.gameObject.SetActive(active);
        }
    }

    private static Text CreateText(string objectName, Transform parent, string text, int size, FontStyle style, TextAnchor alignment, Color color)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);
        Text label = obj.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private static Button CreateButton(string objectName, Transform parent, string text, Color color)
    {
        GameObject root = new GameObject(objectName);
        root.transform.SetParent(parent, false);
        Image image = root.AddComponent<Image>();
        image.color = color;
        Button button = root.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        button.colors = colors;

        Text label = CreateText("Text", root.transform, text, 24, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(label.rectTransform);
        return button;
    }

    private static void SetRect(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

[Serializable]
public class LoginRequestData
{
    public string username;
    public string password;
}

[Serializable]
public class LoginResponseData
{
    public bool success;
    public string message;
    public string token;
}
