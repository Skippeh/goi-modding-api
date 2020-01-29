using System;
using System.Runtime.InteropServices;
using ModAPI.UI.Win32Input.Data;
using ModAPI.UI.Win32Input.Enums;
using ModAPI.UI.Win32Input.EventData;
using UnityEngine;
using Xilium.CefGlue;

namespace ModAPI.UI.Win32Input
{
    internal static class RawInput
    {
        /// <summary>
        /// Invoked when a button on the keyboard was pressed.
        /// </summary>
        public static event Action<RawKeyEventData> RawKey;

        /// <summary>
        /// Invoked when the window is restored from being minimized or if the window size changed.
        /// </summary>
        public static event Action SizeChanged;

        /// <summary>
        /// Invoked when...
        /// </summary>
        public static event Action CaptureLost;

        /// <summary>
        /// Invoked when the mouse is moved.
        /// </summary>
        public static event Action<MouseEventData> MouseMove;

        /// <summary>
        /// Invoked when a mouse button is pushed down.
        /// </summary>
        public static event Action<MouseEventData> MouseDown;

        /// <summary>
        /// Invoked when a mouse button is released.
        /// </summary>
        public static event Action<MouseEventData> MouseUp;

        /// <summary>
        /// Invoked when the mouse wheel is scrolled.
        /// </summary>
        public static event Action<MouseEventData> MouseScroll;

        /// <summary>
        /// Invoked when an X button is released. Note that the event is called even if the window has lost focus after pressing down the button.
        /// Parameter is true if the button is forward, otherwise false.
        /// </summary>
        public static event Action<bool> XButtonUp;

        /// <summary>
        /// Whether the service is running and input messages are being processed.
        /// </summary>
        public static bool IsRunning => hookPtr != IntPtr.Zero;
        /// <summary>
        /// Whether handled input messages should not be propagated further.
        /// </summary>
        public static bool InterceptMessages { get; set; }

        private static IntPtr hookPtr = IntPtr.Zero;

        /// <summary>
        /// Initializes the service and starts processing input messages.
        /// </summary>
        /// <returns>Whether the service started successfully.</returns>
        public static bool Start ()
        {
            if (IsRunning) return true;
            return SetHook();
        }

        /// <summary>
        /// Terminates the service and stops processing input messages.
        /// </summary>
        public static void Stop ()
        {
            RemoveHook();
        }

        public static bool IsKeyDown(VK vKey)
        {
            return (Win32API.GetKeyState(vKey) & 0x8000) != 0;
        }

        private static bool SetHook ()
        {
            if (hookPtr == IntPtr.Zero)
            {
                hookPtr = Win32API.SetWindowsHookEx(HookType.WH_GETMESSAGE, HandleHookProc, IntPtr.Zero, (int) Win32API.GetCurrentThreadId());
            }

            return hookPtr != IntPtr.Zero;
        }

        private static void RemoveHook ()
        {
            if (hookPtr != IntPtr.Zero)
            {
                Win32API.UnhookWindowsHookEx(hookPtr);
                hookPtr = IntPtr.Zero;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Win32API.HookProc))]
        private static int HandleHookProc (int code, IntPtr wParam, ref MSG lParam)
        {
            if (code < 0) return Win32API.CallNextHookEx(hookPtr, code, wParam, ref lParam);
            bool intercept = false;

            switch (lParam.message)
            {
                case WM.SYSCHAR:
                case WM.SYSKEYDOWN:
                case WM.SYSKEYUP:
                case WM.KEYDOWN:
                case WM.KEYUP:
                case WM.CHAR:
                {
                    var eventData = new RawKeyEventData
                    {
                        Message = lParam.message,
                        WindowsKeyCode = (int)lParam.wParam,
                        NativeKeyCode = (int)lParam.lParam
                    };

                    intercept = HandleRawKey(eventData);

                    break;
                }
                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.LBUTTONDBLCLK:
                case WM.RBUTTONDBLCLK:
                case WM.MBUTTONDBLCLK:
                case WM.LBUTTONUP:
                case WM.RBUTTONUP:
                case WM.MBUTTONUP:
                case WM.MOUSEMOVE:
                case WM.MOUSELEAVE:
                case WM.MOUSEWHEEL:
                case WM.XBUTTONDOWN:
                case WM.XBUTTONUP:
                case WM.XBUTTONDBLCLK:
                {
                    intercept = HandleMouseEvent(lParam);
                    break;
                }
                case WM.CAPTURECHANGED:
                case WM.CANCELMODE:
                {
                    CaptureLost?.Invoke();
                    break;
                }
                case WM.SIZE:
                {
                    SizeChanged?.Invoke();
                    break;
                }
            }

            return intercept || InterceptMessages ? 1 : Win32API.CallNextHookEx(hookPtr, 0, wParam, ref lParam);
        }

        private static readonly MouseClick lastClick = new MouseClick();
        private static readonly uint DoubleClickTime = Win32API.GetDoubleClickTime();
        private static readonly int DoubleClickDistanceX = Win32API.GetSystemMetrics(SM.CXDOUBLECLK);
        private static readonly int DoubleClickDistanceY = Win32API.GetSystemMetrics(SM.CYDOUBLECLK);

        private static bool HandleMouseEvent(MSG msg)
        {
            long currentTime = Win32API.GetMessageTime();
            Int32POINT[] coords = GetMouseCoordinates(msg.hwnd, msg.message, msg.lParam);
            Int32POINT screenCoords = coords[0];
            Int32POINT clientCoords = coords[1];

            // Update lastClick
            switch (msg.message)
            {
                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.LBUTTONDBLCLK:
                case WM.RBUTTONDBLCLK:
                case WM.MBUTTONDBLCLK:
                case WM.MOUSEMOVE:
                case WM.MOUSELEAVE:
                {
                    int deltaX = Math.Abs(lastClick.ClientPoint.x - clientCoords.x);
                    int deltaY = Math.Abs(lastClick.ClientPoint.y - clientCoords.y);
                    long elapsedTime = currentTime - lastClick.Time;

                    if (deltaX > DoubleClickDistanceX || deltaY > DoubleClickDistanceY || elapsedTime > DoubleClickTime)
                    {
                        lastClick.ClickCount = 0;
                    }

                    lastClick.ScreenPoint = screenCoords;
                    lastClick.ClientPoint = clientCoords;
                    break;
                }
            }

            // Update lastClick click count and time
            switch (msg.message)
            {
                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.LBUTTONDBLCLK:
                case WM.RBUTTONDBLCLK:
                case WM.MBUTTONDBLCLK:
                {
                    lastClick.Time = currentTime;
                    var cefButton = GetCefButton(msg.message);

                    if (cefButton == lastClick.Button)
                    {
                        lastClick.ClickCount += 1;

                        if (lastClick.ClickCount > 3)
                            lastClick.ClickCount = 1; // Reset to 1 if clicking more than 3 times
                    }
                    else
                    {
                        lastClick.ClickCount = 1;
                    }

                    lastClick.Button = cefButton;
                    
                    break;
                }
            }

            var eventData = new MouseEventData
            {
                Button = lastClick.Button,
                ScreenPosition = new Vector2(screenCoords.x, screenCoords.y),
                WindowPosition = new Vector2(clientCoords.x, clientCoords.y),
                ClickCount = lastClick.ClickCount,
                OriginalMessage = msg
            };

            switch (msg.message)
            {
                // Invoke mouse down event
                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.LBUTTONDBLCLK:
                case WM.RBUTTONDBLCLK:
                case WM.MBUTTONDBLCLK:
                {
                    MouseDown?.Invoke(eventData);
                    break;
                }
                // Invoke mouse up event
                case WM.LBUTTONUP:
                case WM.RBUTTONUP:
                case WM.MBUTTONUP:
                {
                    MouseUp?.Invoke(eventData);
                    break;
                }
                // Invoke mouse move event
                case WM.MOUSEMOVE:
                case WM.MOUSELEAVE:
                {
                    // Invoke mouse move event with LeftClientArea set to true
                    if (msg.message == WM.MOUSELEAVE)
                        eventData.LeftClientArea = true;
                    
                    MouseMove?.Invoke(eventData);
                    break;
                }
                // Invoke mouse scroll event
                case WM.MOUSEWHEEL:
                {
                    var delta = GetPoint(msg.wParam).y;
                    eventData.ScrollDelta = delta;
                    MouseScroll?.Invoke(eventData);
                    break;
                }
                // Invoke forward/backward mouse click event
                case WM.XBUTTONUP:
                {
                    int xButton = GetPoint(msg.wParam).y;
                    XButtonUp?.Invoke(xButton == 2);
                    break;
                }
            }

            return false;
        }

        private static CefMouseButtonType GetCefButton(WM message)
        {
            switch (message)
            {
                case WM.LBUTTONDOWN:
                case WM.LBUTTONDBLCLK:
                    return CefMouseButtonType.Left;
                case WM.RBUTTONDOWN:
                case WM.RBUTTONDBLCLK:
                    return CefMouseButtonType.Right;
                case WM.MBUTTONDOWN:
                case WM.MBUTTONDBLCLK:
                    return CefMouseButtonType.Middle;
            }

            Console.WriteLine($"Unknown CefButton: {message}");
            return 0;
        }

        private static Int32POINT[] GetMouseCoordinates(IntPtr hWnd, WM message, IntPtr lParam)
        {
            Int32POINT coords = GetPoint(lParam);
            Int32POINT screenCoords = new Int32POINT();
            Int32POINT clientCoords = new Int32POINT();

            switch (message)
            {
                // Screen coordinates
                case WM.MOUSEWHEEL:
                {
                    screenCoords = coords;
                    Win32API.ScreenToClient(hWnd, ref clientCoords);
                    
                    // Client coordinates equal the negative offset from the top left corner of the screen. Add screen coordinates to get client coordinates.
                    clientCoords.x += screenCoords.x;
                    clientCoords.y += screenCoords.y;
                    
                    break;
                }
                // Client area coordinates
                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.LBUTTONDBLCLK:
                case WM.RBUTTONDBLCLK:
                case WM.MBUTTONDBLCLK:
                case WM.LBUTTONUP:
                case WM.RBUTTONUP:
                case WM.MBUTTONUP:
                case WM.MOUSEMOVE:
                {
                    clientCoords = coords;
                    Win32API.ClientToScreen(hWnd, ref screenCoords);
                    
                    // Screen coordinates equal the top left corner of the window. Add client coordinates to get mouse screen coordinates.
                    screenCoords.x += clientCoords.x;
                    screenCoords.y += clientCoords.y;
                    break;
                }
            }

            return new[] {screenCoords, clientCoords};
        }

        private static Int32POINT GetPoint(IntPtr lParam)
        {
            int x = unchecked((short) (long) lParam);
            int y = unchecked((short) ((long) lParam >> 16));

            return new Int32POINT()
            {
                x = x,
                y = y
            };
        }

        private static bool HandleRawKey(RawKeyEventData eventData)
        {
            RawKey?.Invoke(eventData);
            return eventData.Intercept;
        }

        public static CefEventFlags GetCefKeyboardModifiers(VK vKey, uint lParam)
        {
            CefEventFlags modifiers = 0;

            if (IsKeyDown(VK.SHIFT))
                modifiers |= CefEventFlags.ShiftDown;
            if (IsKeyDown(VK.CONTROL))
                modifiers |= CefEventFlags.ControlDown;
            if (IsKeyDown(VK.MENU))
                modifiers |= CefEventFlags.AltDown;

            // Low bit set from GetKeyState indicates "toggled".
            if ((Win32API.GetKeyState(VK.NUMLOCK) & 1) != 0)
                modifiers |= CefEventFlags.NumLockOn;
            if ((Win32API.GetKeyState(VK.CAPITAL) & 1) != 0)
                modifiers |= CefEventFlags.CapsLockOn;

            switch (vKey)
            {
                case VK.RETURN:
                    if (((lParam >> 16) & (uint) KF.EXTENDED) != 0)
                        modifiers |= CefEventFlags.IsKeyPad;
                    break;
                case VK.INSERT:
                case VK.DELETE:
                case VK.HOME:
                case VK.END:
                case VK.PRIOR:
                case VK.NEXT:
                case VK.UP:
                case VK.DOWN:
                case VK.LEFT:
                case VK.RIGHT:
                    if (((lParam >> 16) & (uint) KF.EXTENDED) == 0)
                        modifiers |= CefEventFlags.IsKeyPad;
                    break;
                case VK.NUMLOCK:
                case VK.NUMPAD0:
                case VK.NUMPAD1:
                case VK.NUMPAD2:
                case VK.NUMPAD3:
                case VK.NUMPAD4:
                case VK.NUMPAD5:
                case VK.NUMPAD6:
                case VK.NUMPAD7:
                case VK.NUMPAD8:
                case VK.NUMPAD9:
                case VK.DIVIDE:
                case VK.MULTIPLY:
                case VK.SUBTRACT:
                case VK.ADD:
                case VK.DECIMAL:
                case VK.CLEAR:
                    modifiers |= CefEventFlags.IsKeyPad;
                    break;
                case VK.SHIFT:
                    if (IsKeyDown(VK.LSHIFT))
                        modifiers |= CefEventFlags.IsLeft;
                    else if (IsKeyDown(VK.RSHIFT))
                        modifiers |= CefEventFlags.IsRight;
                    break;
                case VK.CONTROL:
                    if (IsKeyDown(VK.LCONTROL))
                        modifiers |= CefEventFlags.IsLeft;
                    else if (IsKeyDown(VK.RCONTROL))
                        modifiers |= CefEventFlags.IsRight;
                    break;
                case VK.MENU:
                    if (IsKeyDown(VK.LMENU))
                        modifiers |= CefEventFlags.IsLeft;
                    else if (IsKeyDown(VK.RMENU))
                        modifiers |= CefEventFlags.IsRight;
                    break;
                case VK.LWIN:
                    modifiers |= CefEventFlags.IsLeft;
                    break;
                case VK.RWIN:
                    modifiers |= CefEventFlags.IsRight;
                    break;
            }
            
            return modifiers;
        }

        public static CefEventFlags GetCefMouseModifiers(uint wParam)
        {
            CefEventFlags modifiers = CefEventFlags.None;

            if ((wParam & (uint) MK.CONTROL) != 0)
                modifiers |= CefEventFlags.ControlDown;
            if ((wParam & (uint) MK.SHIFT) != 0)
                modifiers |= CefEventFlags.ShiftDown;
            if (IsKeyDown(VK.MENU))
                modifiers |= CefEventFlags.AltDown;
            if ((wParam & (uint) MK.LBUTTON) != 0)
                modifiers |= CefEventFlags.LeftMouseButton;
            if ((wParam & (uint) MK.MBUTTON) != 0)
                modifiers |= CefEventFlags.MiddleMouseButton;
            if ((wParam & (uint) MK.RBUTTON) != 0)
                modifiers |= CefEventFlags.RightMouseButton;
            
            // Low bit set from GetKeyState indicates "toggled".
            if ((Win32API.GetKeyState(VK.NUMLOCK) & 1) != 0)
                modifiers |= CefEventFlags.NumLockOn;
            if ((Win32API.GetKeyState(VK.CAPITAL) & 1) != 0)
                modifiers |= CefEventFlags.CapsLockOn;
            
            return modifiers;
        }
    }

    internal class MouseClick
    {
        public Int32POINT ScreenPoint = new Int32POINT {x = 0, y = 0};
        public Int32POINT ClientPoint = new Int32POINT {x = 0, y = 0};
        public CefMouseButtonType Button;
        public long Time;
        public uint ClickCount;
    }
}
