using System.Text.RegularExpressions; // 用来检查用户名和密码格式
using TMPro; // 使用 TMP_InputField 和 TMP_Text
using UnityEngine; // 使用 Unity 基础功能
using UnityEngine.UI; // 使用 Button

// 这个脚本负责注册界面的本地输入检查
// 本地检查失败时，把错误原因显示到 MessageText 显示条
// 本地检查通过后，只代表可以继续交给服务器注册脚本，不代表真正注册成功
public class RegisterLocalValidator : MonoBehaviour
{
    [Header("注册输入框")]
    public TMP_InputField UsernameInput; // 用户名输入框
    public TMP_InputField PasswordInput; // 密码输入框
    public TMP_InputField ConfirmPasswordInput; // 确认密码输入框

    [Header("注册按钮")]
    public Button RegisterButton; // 注册按钮

    [Header("提示显示条")]
    public TMP_Text MessageText; // 显示错误提示的文字

    private void Start()
    {
        // 启动时先清空提示
        SetMessage("");

        // 检查注册按钮是否绑定
        if (RegisterButton == null)
        {
            SetMessage("注册按钮没有绑定");
            return;
        }

        // 点击注册按钮时执行本地检查
        RegisterButton.onClick.AddListener(CheckRegisterInput);
    }

    private void OnDestroy()
    {
        // 销毁时取消按钮监听
        if (RegisterButton != null)
        {
            RegisterButton.onClick.RemoveListener(CheckRegisterInput);
        }
    }

    // 注册按钮点击后执行
    private void CheckRegisterInput()
    {
        // 检查输入框是否全部绑定
        if (UsernameInput == null || PasswordInput == null || ConfirmPasswordInput == null)
        {
            ShowError("注册输入框没有全部绑定");
            return;
        }

        // 读取输入内容
        string username = UsernameInput.text.Trim();
        string password = PasswordInput.text;
        string confirmPassword = ConfirmPasswordInput.text;

        // 检查用户名
        if (string.IsNullOrEmpty(username))
        {
            ShowError("用户名不能为空");
            return;
        }

        if (username.Length < 3 || username.Length > 16)
        {
            ShowError("用户名长度需要在 3 到 16 位之间");
            return;
        }

        if (!Regex.IsMatch(username, @"^[A-Za-z0-9_]+$"))
        {
            ShowError("用户名只能使用英文、数字和下划线");
            return;
        }

        // 检查密码
        if (string.IsNullOrEmpty(password))
        {
            ShowError("密码不能为空");
            return;
        }

        if (password.Length < 6 || password.Length > 20)
        {
            ShowError("密码长度需要在 6 到 20 位之间");
            return;
        }

        if (Regex.IsMatch(password, @"\s"))
        {
            ShowError("密码不能包含空格");
            return;
        }

        if (!Regex.IsMatch(password, @"[A-Za-z]") || !Regex.IsMatch(password, @"[0-9]"))
        {
            ShowError("密码需要同时包含英文和数字");
            return;
        }

        // 检查确认密码
        if (string.IsNullOrEmpty(confirmPassword))
        {
            ShowError("确认密码不能为空");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("两次输入的密码不一致");
            return;
        }

        // 本地检查通过
        SetMessage("本地检查通过，准备提交注册");
        Debug.Log("本地检查通过，可以发送注册请求");
    }

    // 显示错误提示
    private void ShowError(string message)
    {
        SetMessage(message);
        Debug.LogWarning(message);
    }

    // 设置显示条文字
    private void SetMessage(string message)
    {
        if (MessageText != null)
        {
            MessageText.text = message;
        }
    }
}