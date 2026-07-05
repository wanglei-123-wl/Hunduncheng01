using UnityEngine; // 引入 Unity 基础功能
using UnityEngine.UI; // 引入 Unity 按钮功能

public class LoginPanelSwitch : MonoBehaviour // 定义登录注册切换脚本
{ // 类开始
    public GameObject Login; // 对应子物体 Login
    public GameObject Register; // 对应子物体 Register
    public Button RegisterButton01; // 对应 Login 里面的注册按钮 Button

    private void Start() // 游戏开始时执行
    { // Start 开始
        Login.SetActive(true); // 默认显示 Login
        Register.SetActive(false); // 默认隐藏 Register
        RegisterButton01.onClick.AddListener(ShowRegister); // 点击 Button 后执行 ShowRegister
    } // Start 结束

    private void ShowRegister() // 显示注册界面
    { // 方法开始
        Login.SetActive(false); // 隐藏 Login
        Register.SetActive(true); // 显示 Register
    } // 方法结束
} // 类结束