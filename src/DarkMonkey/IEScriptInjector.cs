using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using mshtml;

namespace DarkMonkey
{
    public static class IEScriptInjector
    {
        internal static List<Scriptlet> Scriptlets = null;
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        const int nChars = 1024;
        static StringBuilder buff = new StringBuilder(nChars);

        static string GetWindowClassName(IntPtr hWnd)
        {
            if (GetClassName(hWnd, buff, nChars) > 0)
                return buff.ToString();
            return null;
        }

        public static Action<string> DisplayStatus = null;
        public static Action<bool> OnIEActive = null;
        public static Action<string> OnScriptInject = null;

        static List<Tuple<string, IHTMLDocument2>> IEHtmlDocs = new List<Tuple<string, IHTMLDocument2>>();
        const string ChkAttr = "data-dark-monkey";
        private static int EnumIEWindows(IntPtr hWnd, ref IntPtr lParam)
        {
            int iRet = 1;
            var className = GetWindowClassName(hWnd);
            if (className == "IEFrame" || className == "Internet Explorer_TridentDlgFrame")
            {
                lParam = hWnd;
                var doc = GetHTMLDocument(hWnd);
                if (doc != null)
                {
                    var body = doc.body;
                    if (body != null)
                    {
                        if (body.getAttribute(ChkAttr) is DBNull)
                        {
                            IEHtmlDocs.Add(new Tuple<string, IHTMLDocument2>(doc.url, doc));
                        }
                    }
                }
            }
            return iRet;
        }

        static EnumProc enumIE = new EnumProc(EnumIEWindows);
        static IntPtr desktopHwnd = GetDesktopWindow();
        public static void MonitorIE()
        {
            IntPtr fgWin = GetForegroundWindow();
            //When IE is the active window
            var fgWinClass = GetWindowClassName(fgWin);
            if (fgWinClass != "IEFrame" && fgWinClass != "Internet Explorer_TridentDlgFrame")
            {
                OnIEActive?.Invoke(false);
                return;
            }
            OnIEActive?.Invoke(true);

            if (!Scriptlets.Any(o => !o.Disabled)) return;

            IEHtmlDocs.Clear();
            var hWnd = desktopHwnd;
            EnumChildWindows(hWnd, enumIE, ref hWnd);
            IEHtmlDocs.ToList()
                .ForEach(o =>
                {
                    var url = o.Item1;
                    var doc = o.Item2;
                    Scriptlets.Where(s => !s.Disabled).ToList().ForEach(s =>
                    {
                        try
                        {
                            if (Regex.IsMatch(url, s.UrlMatch))
                            {
                                doc.body.setAttribute(ChkAttr, "Y");
                                OnScriptInject?.Invoke(s.UrlMatch);
                                HTMLWindow2 win = (HTMLWindow2)doc.Script;
                                win.execScript(s.Script, "javascript");
                                DisplayStatus?.Invoke($"Inject [{s.Name}]...");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Script Error: " + ex.Message);
                        }

                    });
                });

        }

        #region API CALLS
        //REF: https://stackoverflow.com/a/51341944/288936
        [DllImport("user32.dll", EntryPoint = "GetClassNameA")]
        public static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);
        /*delegate to handle EnumChildWindows*/
        public delegate int EnumProc(IntPtr hWnd, ref IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, EnumProc lpEnumFunc, ref IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageA")]
        public static extern int RegisterWindowMessage(string lpString);
        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutA")]
        public static extern int SendMessageTimeout(IntPtr hwnd, int msg, int wParam, int lParam, int fuFlags, int uTimeout, out int lpdwResult);
        [DllImport("OLEACC.dll")]
        public static extern int ObjectFromLresult(int lResult, ref Guid riid, int wParam, ref IHTMLDocument2 ppvObject);
        public const int SMTO_ABORTIFHUNG = 0x2;
        public static Guid IID_IHTMLDocument = new Guid("626FC520-A41E-11CF-A731-00A0C9082637");


        private static int FindIEServer(IntPtr hWnd, ref IntPtr lParam)
        {
            int iRet = 1;
            var clsName = GetWindowClassName(hWnd);
            if (clsName == "Internet Explorer_Server")
            {
                lParam = hWnd;
                iRet = 0;
            }
            return iRet;
        }
        static EnumProc findIESvr = new EnumProc(FindIEServer);
        public static IHTMLDocument2 GetHTMLDocument(IntPtr hWnd)
        {
            var hFound = hWnd;
            EnumChildWindows(hWnd, findIESvr, ref hFound);
            if (hFound != IntPtr.Zero)
            {
                IHTMLDocument2 document = null;
                int iMsg = 0;
                int iRes = 0;

                iMsg = RegisterWindowMessage("WM_HTML_GETOBJECT");
                if (iMsg != 0)
                {
                    SendMessageTimeout(hFound, iMsg, 0, 0, SMTO_ABORTIFHUNG, 1000, out iRes);
                    if (iRes != 0)
                    {
                        int hr = ObjectFromLresult(iRes, ref IID_IHTMLDocument, 0, ref document);
                        return document;
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
