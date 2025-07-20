using System.Collections.Generic;

namespace RYCBEditorX
{
    public partial class App
    {
        private string GetLoadingTip(string language, int index)
        {
            return language switch
            {
                "zh-CN" =>
             new List<string>()
             { "RYCBEditor正在启动...", "初始化...", "加载用户配置...", "加载异常处理...", "加载本地化配置...", "加载模块...",
                 "尝试连接网络", "即将完成..." }[index],
                "en-US" =>
             new List<string>()
             { "RYCBEditor is starting up...", "Initializing...", "Loading User Profiles...",
                 "Loading Exception Handling...", "Loading Localizations...", "Loading Modules...", 
                 "Trying to connect to the network", "Coming to complete..." }[index],
                "ja_jp" =>
             new List<string>()
             { "RYCBEditorが起動しています...", "初期化...", "ユーザー構成をロード...",
                 "ロード例外処理...", "ローカライズされた構成のロード...", "モジュールのロード...","ネット接続を試みました" , "完了間近..." }[index],
                "zh-TD" =>
             new List<string>()
             { "RYCBEditor正在啟動...", "初始化...", "加載用戶配寘...", "加載異常處理...", "加載當地語系化配寘...",
                 "加載模塊...", "嘗試連接網路", "即將完成..." }[index],
                _ => ""
            };
        }
    }
}
