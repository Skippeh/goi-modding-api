using System;
using ModAPI.UI.CEF;
using ModAPI.UI.Win32Input;
using ModAPI.UI.Win32Input.Enums;
using ModAPI.UI.Win32Input.EventData;
using UnityEngine;
using Xilium.CefGlue;

namespace ModAPI.UI
{
    internal class FullscreenBrowserInputManager : MonoBehaviour
    {
        public OffScreenClient Client;

        private CefBrowserHost BrowserHost => Client.BrowserHost;

        private void OnEnable()
        {
            RawInput.RawKey += OnRawKey;
            RawInput.MouseMove += OnRawMouseMove;
            RawInput.MouseDown += OnRawMouseDown;
            RawInput.MouseUp += OnRawMouseUp;
            RawInput.MouseScroll += OnRawMouseScroll;
            RawInput.SizeChanged += OnRawSizeChanged;
            RawInput.CaptureLost += OnRawCaptureLost;
            RawInput.XButtonUp += OnRawXButtonUp;
        }

        private void OnDisable()
        {
            RawInput.RawKey -= OnRawKey;
            RawInput.MouseMove -= OnRawMouseMove;
            RawInput.MouseDown -= OnRawMouseDown;
            RawInput.MouseUp -= OnRawMouseUp;
            RawInput.MouseScroll -= OnRawMouseScroll;
            RawInput.SizeChanged -= OnRawSizeChanged;
            RawInput.CaptureLost -= OnRawCaptureLost;
            RawInput.XButtonUp -= OnRawXButtonUp;
        }

        private void SendMouseMoveEvent(MouseEventData eventData, CefEventFlags modifiers)
        {
            var mouseEvent = new CefMouseEvent((int) eventData.WindowPosition.x, (int) eventData.WindowPosition.y, modifiers);
            BrowserHost.SendMouseMoveEvent(mouseEvent, eventData.LeftClientArea);
        }

        private void SendScrollWheelEvent(MouseEventData eventData, CefEventFlags modifiers)
        {
            var mouseEvent = new CefMouseEvent((int) eventData.WindowPosition.x, (int) eventData.WindowPosition.y, modifiers);
            BrowserHost.SendMouseWheelEvent(mouseEvent, 0, eventData.ScrollDelta);
        }

        private void SendMouseClick(MouseEventData eventData, CefEventFlags modifiers, bool buttonDown)
        {
            var mouseEvent = new CefMouseEvent((int) eventData.WindowPosition.x, (int) eventData.WindowPosition.y, modifiers);
            BrowserHost.SendMouseClickEvent(mouseEvent, eventData.Button, !buttonDown, (int) eventData.ClickCount);
        }

        private void SendKeyEvent(RawKeyEventData eventData)
        {
            if (BrowserHost == null)
                return;

            var keyEvent = new CefKeyEvent
            {
                WindowsKeyCode = eventData.WindowsKeyCode,
                NativeKeyCode = eventData.NativeKeyCode,
                IsSystemKey = eventData.Message == WM.SYSCHAR ||
                              eventData.Message == WM.SYSKEYDOWN ||
                              eventData.Message == WM.SYSKEYUP,
                Modifiers = RawInput.GetCefKeyboardModifiers((VK) eventData.WindowsKeyCode, (uint) eventData.NativeKeyCode)
            };

            if (eventData.Message == WM.KEYDOWN || eventData.Message == WM.SYSKEYDOWN)
            {
                keyEvent.EventType = CefKeyEventType.RawKeyDown;
            }
            else if (eventData.Message == WM.KEYUP || eventData.Message == WM.SYSKEYUP)
            {
                keyEvent.EventType = CefKeyEventType.KeyUp;
            }
            else
            {
                keyEvent.EventType = CefKeyEventType.Char;
            }

            BrowserHost.SendKeyEvent(keyEvent);
        }

        private void OnApplicationFocus(bool focused)
        {
            Console.WriteLine($"Focus changed: {focused}");
            BrowserHost?.SendFocusEvent(focused);
        }

        private void OnRawKey(RawKeyEventData eventData)
        {
            if (BrowserHost == null)
                return;

            if (eventData.Message == WM.KEYDOWN && eventData.WindowsKeyCode == (uint) VK.F5)
            {
                // Refresh current page
                Console.WriteLine("Reloading UI...");
                BrowserHost.GetBrowser().ReloadIgnoreCache();
                return;
            }
            
            if (!Cursor.visible)
                return;

            SendKeyEvent(eventData);
        }

        private void OnRawMouseMove(MouseEventData eventData)
        {
            if (BrowserHost == null || !Cursor.visible)
                return;

            SendMouseMoveEvent(eventData, RawInput.GetCefMouseModifiers((uint) eventData.OriginalMessage.wParam));
        }

        private void OnRawMouseDown(MouseEventData eventData)
        {
            if (BrowserHost == null || !Cursor.visible)
                return;

            var cefMouseModifiers = RawInput.GetCefMouseModifiers((uint) eventData.OriginalMessage.wParam);
            SendMouseClick(eventData, cefMouseModifiers, true);
        }

        private void OnRawMouseUp(MouseEventData eventData)
        {
            if (BrowserHost == null || !Cursor.visible)
                return;

            SendMouseClick(eventData, RawInput.GetCefMouseModifiers((uint) eventData.OriginalMessage.wParam), false);
        }

        private void OnRawMouseScroll(MouseEventData eventData)
        {
            if (BrowserHost == null || !Cursor.visible)
                return;

            SendScrollWheelEvent(eventData, RawInput.GetCefMouseModifiers((uint) eventData.OriginalMessage.wParam));
        }

        private void OnRawSizeChanged()
        {
            BrowserHost?.WasResized();
        }

        private void OnRawCaptureLost()
        {
            BrowserHost?.SendCaptureLostEvent();
        }

        private void OnRawXButtonUp(bool navigateForward)
        {
            if (Application.isFocused && Cursor.visible)
            {
                if (navigateForward)
                    BrowserHost?.GetBrowser().GoForward();
                else
                    BrowserHost?.GetBrowser().GoBack();
            }
        }
    }
}