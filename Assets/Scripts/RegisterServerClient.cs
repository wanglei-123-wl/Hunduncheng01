using System; // 使用 Serializable
using System.Collections; // 使用 Coroutine
using System.Text; // 使用 UTF8 编码
using TMPro; // 使用 TMP_Text 提示文字
using UnityEngine; // 使用 Unity 基础功能
using UnityEngine.Networking; // 使用 UnityWebRequest
using UnityEngine.UI; // 使用 Button

// RegisterServerClient 负责注册请求和服务器通信
// 本地判断脚本检查通过后，调用 SendRegister(username, password)
// 只有服务器返回 success=true，才隐藏 Register，显示 Login
public class RegisterServerClient : MonoBehaviour
{
    [Header("界面")]
    public GameObject Login; // 登录界面
    public GameObject Register; // 注册界面

    [Header("按钮和提示")]
    public Button RegisterButton; // 注册按钮
    public TMP_Text MessageText; // 注册界面的提示文字

    [Header("服务器")]
    public string RegisterUrl = "http://127.0.0.1:8080/api/register"; // 注册接口地址

    private Coroutine registerCoroutine; // 当前注册请求

    // 本地判断通过后调用这个方法
    public void SendRegister(string username, string password)
    {
        // 防止重复提交
        if (registerCoroutine != null)
        {
            return;
        }

        // 启动注册请求
        registerCoroutine = StartCoroutine(RegisterRequest(username, password));
    }

    // 向服务器发送注册请求
    private IEnumerator RegisterRequest(string username, string password)
    {
        // 显示请求状态
        SetMessage("注册中...");

        // 请求期间禁用按钮，防止连点
        SetRegisterButton(false);

        // 组装请求数据
        RegisterRequestData requestData = new RegisterRequestData
        {
            username = username,
            password = password
        };

        // 转成 JSON
        string json = JsonUtility.ToJson(requestData);

        // 转成字节数据
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // 创建 POST 请求
        using (UnityWebRequest request = new UnityWebRequest(RegisterUrl, "POST"))
        {
            // 设置上传内容
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // 设置接收内容
            request.downloadHandler = new DownloadHandlerBuffer();

            // 告诉服务器这是 JSON
            request.SetRequestHeader("Content-Type", "application/json");

            // 设置超时时间
            request.timeout = 10;

            // 发送请求并等待服务器返回
            yield return request.SendWebRequest();

            // 请求结束，恢复按钮
            SetRegisterButton(true);

            // 清空当前请求
            registerCoroutine = null;

            // 网络失败或服务器连不上
            if (request.result != UnityWebRequest.Result.Success)
            {
                SetMessage("无法连接服务器，请稍后再试");
                yield break;
            }

            // 解析服务器返回
            RegisterResponseData responseData = null;

            try
            {
                responseData = JsonUtility.FromJson<RegisterResponseData>(request.downloadHandler.text);
            }
            catch (Exception)
            {
                SetMessage("服务器返回格式错误");
                yield break;
            }

            // 服务器明确返回注册成功
            if (responseData != null && responseData.success)
            {
                SetMessage("注册成功，请登录");
                ShowLoginPanel();
                yield break;
            }

            // 服务器返回注册失败
            string errorMessage = responseData != null ? responseData.message : "注册失败";
            SetMessage(string.IsNullOrEmpty(errorMessage) ? "注册失败" : errorMessage);
        }
    }

    // 注册成功后返回登录界面
    private void ShowLoginPanel()
    {
        if (Register != null)
        {
            Register.SetActive(false);
        }

        if (Login != null)
        {
            Login.SetActive(true);
        }
    }

    // 设置注册按钮状态
    private void SetRegisterButton(bool canClick)
    {
        if (RegisterButton != null)
        {
            RegisterButton.interactable = canClick;
        }
    }

    // 设置提示文字
    private void SetMessage(string message)
    {
        if (MessageText != null)
        {
            MessageText.text = message;
        }
    }
}

// 发给服务器的注册数据
[Serializable]
public class RegisterRequestData
{
    public string username;
    public string password;
}

// 服务器返回的注册结果
[Serializable]
public class RegisterResponseData
{
    public bool success;
    public string message;
}