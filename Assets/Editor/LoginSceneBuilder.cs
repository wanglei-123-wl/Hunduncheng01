using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LoginSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/LoginScene.unity";

    [MenuItem("Tools/Login/Create Login Scene")]
    public static void CreateLoginScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "LoginScene";

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.1f, 0.13f);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();

        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject background = CreatePanel("Background", canvasObject.transform, new Color(0.06f, 0.075f, 0.095f, 1f));
        Stretch(background.GetComponent<RectTransform>());

        GameObject loginPanel = CreatePanel("LoginPanel", canvasObject.transform, new Color(0.14f, 0.16f, 0.19f, 0.96f));
        SetRect(loginPanel.GetComponent<RectTransform>(), 0f, 0f, 520f, 420f);

        Text titleText = CreateText("TitleText", loginPanel.transform, "Login", 46, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        SetRect(titleText.rectTransform, 0f, 145f, 420f, 70f);

        InputField usernameInput = CreateInputField("UsernameInput", loginPanel.transform, "username", false);
        SetRect(usernameInput.GetComponent<RectTransform>(), 0f, 65f, 400f, 58f);

        InputField passwordInput = CreateInputField("PasswordInput", loginPanel.transform, "password", true);
        SetRect(passwordInput.GetComponent<RectTransform>(), 0f, -10f, 400f, 58f);

        Button loginButton = CreateButton("LoginButton", loginPanel.transform, "Login");
        SetRect(loginButton.GetComponent<RectTransform>(), 0f, -92f, 400f, 60f);

        Text messageText = CreateText("MessageText", loginPanel.transform, "", 24, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.35f));
        SetRect(messageText.rectTransform, 0f, -165f, 430f, 48f);

        LoginUI login = loginPanel.AddComponent<LoginUI>();
        login.SetReferences(usernameInput, passwordInput, loginButton, messageText);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Created login scene: " + ScenePath);
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image image = obj.AddComponent<Image>();
        image.color = color;
        return obj;
    }

    private static Text CreateText(string name, Transform parent, string text, int size, FontStyle style, TextAnchor alignment, Color color)
    {
        GameObject obj = new GameObject(name);
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

    private static InputField CreateInputField(string name, Transform parent, string placeholder, bool password)
    {
        GameObject root = CreatePanel(name, parent, new Color(0.93f, 0.95f, 0.98f, 1f));
        InputField input = root.AddComponent<InputField>();
        input.transition = Selectable.Transition.ColorTint;
        input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
        input.inputType = password ? InputField.InputType.Password : InputField.InputType.Standard;
        input.lineType = InputField.LineType.SingleLine;
        input.shouldActivateOnSelect = true;
        input.caretBlinkRate = 0.85f;
        input.caretWidth = 2;
        input.customCaretColor = true;
        input.caretColor = new Color(0.08f, 0.09f, 0.11f, 1f);

        Text placeholderText = CreateText("Placeholder", root.transform, placeholder, 24, FontStyle.Italic, TextAnchor.MiddleLeft, new Color(0.45f, 0.48f, 0.52f));
        Stretch(placeholderText.rectTransform);
        placeholderText.rectTransform.offsetMin = new Vector2(18f, 6f);
        placeholderText.rectTransform.offsetMax = new Vector2(-18f, -6f);

        Text inputText = CreateText("Text", root.transform, "", 24, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.08f, 0.09f, 0.11f));
        Stretch(inputText.rectTransform);
        inputText.rectTransform.offsetMin = new Vector2(18f, 6f);
        inputText.rectTransform.offsetMax = new Vector2(-18f, -6f);

        input.textComponent = inputText;
        input.placeholder = placeholderText;
        return input;
    }

    private static Button CreateButton(string name, Transform parent, string text)
    {
        GameObject root = CreatePanel(name, parent, new Color(0.12f, 0.45f, 0.9f, 1f));
        Button button = root.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.45f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.2f, 0.55f, 1f, 1f);
        colors.pressedColor = new Color(0.08f, 0.34f, 0.72f, 1f);
        colors.disabledColor = new Color(0.36f, 0.38f, 0.42f, 1f);
        button.colors = colors;

        Text label = CreateText("Text", root.transform, text, 28, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        Stretch(label.rectTransform);
        return button;
    }

    private static void AddSceneToBuildSettings(string path)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == path)
            {
                scenes[i].enabled = true;
                EditorBuildSettings.scenes = scenes;
                return;
            }
        }

        EditorBuildSettingsScene[] updatedScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
        {
            updatedScenes[i] = scenes[i];
        }

        updatedScenes[updatedScenes.Length - 1] = new EditorBuildSettingsScene(path, true);
        EditorBuildSettings.scenes = updatedScenes;
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
