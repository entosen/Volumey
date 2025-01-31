﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Volumey.Helper
{
    static class NativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        internal static extern int ExtractIconEx(string lpszFile, int nIconIndex, out IntPtr phiconLarge, IntPtr phiconSmall, int nIcons);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        internal static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr phiconLarge, out IntPtr phiconSmall, int nIcons);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegisterWindowMessage(string message);
        
        /// <summary>
        /// Use with LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020 flag to extract resources
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <param name="handle"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibraryExA(
            [MarshalAs(UnmanagedType.LPStr)] string lpFileName,
            IntPtr handle, uint flag);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int LoadString(IntPtr hInstance, int ID, StringBuilder lpBuffer, int nBufferMax);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// Registers the active instance of an application for restart.
        /// </summary>
        /// <param name="commandLineArgs">Command line args</param>
        /// <param name="Flags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)] string commandLineArgs, int Flags);

        /// <summary>
        /// Removes the active instance of an application from the restart list.
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int UnregisterApplicationRestart();

        /// <summary>
        /// Enables the application to access the window menu (also known as the system menu or the control menu) for copying and modifying.
        /// </summary>
        /// <param name="hWnd">A handle to the window that will own a copy of the window menu.</param>
        /// <param name="bRevert">If this parameter is FALSE, GetSystemMenu returns a handle to the copy of the window menu currently in use.
        /// The copy is initially identical to the window menu, but it can be modified.
        /// If this parameter is TRUE, GetSystemMenu resets the window menu back to the default state. </param>
        /// <returns>If the bRevert parameter is FALSE, the return value is a handle to a copy of the window menu. If the bRevert parameter is TRUE, the return value is NULL.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        /// <summary>
        /// Enables, disables, or grays the specified menu item.
        /// </summary>
        /// <param name="menuPtr">A handle to the menu.</param>
        /// <param name="uIDEnableItem">The menu item to be enabled, disabled, or grayed, as determined by the uEnable parameter. This parameter specifies an item in a menu bar, menu, or submenu.</param>
        /// <param name="uEnable">Controls the interpretation of the uIDEnableItem parameter and indicate whether the menu item is enabled, disabled, or grayed.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int EnableMenuItem(IntPtr menuPtr, long uIDEnableItem, long uEnable);

        /// <summary>
        /// Extracts string from system DLL
        /// </summary>
        /// <param name="file">System DLL name without it's path</param>
        /// <param name="number">Non-negative index</param>
        /// <returns></returns>
        internal static string ExtractStringFromSystemDLL(string file, int number) 
        {
            IntPtr lib = LoadLibraryExA(file, IntPtr.Zero, 0x00000020);
            StringBuilder result = new StringBuilder(64);
            LoadString(lib, number, result, result.Capacity);
            FreeLibrary(lib);
            return result.ToString();
        }
        
        /// <summary>
        /// Defines a system-wide hot key.
        /// </summary>
        /// <param name="hwnd">A handle to the window that will receive WM_HOTKEY messages generated by the hot key.</param>
        /// <param name="id">The identifier of the hot key. If the hWnd parameter is NULL,
        /// then the hot key is associated with the current thread rather than with a particular window. </param>
        /// <param name="modifiers">The keys that must be pressed in combination with the key specified by the vk parameter in order to generate the WM_HOTKEY message. </param>
        /// <param name="vk">The virtual-key code of the hot key.</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool RegisterHotKey(IntPtr hwnd, int id, int modifiers, uint vk);

        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hwnd">A handle to the window associated with the hot key to be freed.</param>
        /// <param name="id">The identifier of the hot key to be freed.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterHotKey(IntPtr hwnd, int id);
        
        private const UInt32 FLASHW_ALL = 3; //Flash both the window caption and taskbar button.        
        private const UInt32 FLASHW_TIMERNOFG = 12; //Flash continuously until the window comes to the foreground.  

        /// <summary>
        /// Flashes the specified window. It does not change the active state of the window.
        /// </summary>
        /// <param name="pwfi"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        /// <summary>
        /// Flashes the specified window.
        /// </summary>
        /// <param name="handle">A handle to the window to be flashed.</param>
        internal static void FlashWindow(IntPtr handle)
        {
            //Don't flash if the window is active            
            FLASHWINFO info = new FLASHWINFO
            {
                hwnd = handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = UInt32.MaxValue,
                dwTimeout = 0
            };
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            FlashWindowEx(ref info);
        }

        /// <summary>
        /// Contains the flash status for a window and the number of times the system should flash the window.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure, in bytes.
            /// </summary>
            internal UInt32 cbSize;

            /// <summary>
            /// A handle to the window to be flashed. The window can be either opened or minimized.
            /// </summary>
            internal IntPtr hwnd;

            /// <summary>
            /// The flash status. 
            /// </summary>
            internal UInt32 dwFlags;

            /// <summary>
            /// The number of times to flash the window.  
            /// </summary>
            internal UInt32 uCount;

            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.  
            /// </summary>
            internal UInt32 dwTimeout;
        }  
        
        [DllImport("kernel32.dll", SetLastError=true, CharSet = CharSet.Auto)]
        private static extern bool QueryFullProcessImageName([In]IntPtr hProcess, [In]int dwFlags, [Out]StringBuilder lpExeName, ref uint lpdwSize);

        internal static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint) fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength)
                       ? fileNameBuilder.ToString()
                       : null;
        }
        
        [DllImport("user32.dll")]
        public static extern  uint MapVirtualKey(uint uCode, MapType uMapType);
		
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetKeyNameText(int lParam, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder str, int size);
		
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        /// <summary>
        /// Installs an application-defined hook procedure into a hook chain.
        /// </summary>
        /// <param name="id">The type of hook procedure to be installed.</param>
        /// <param name="callback">A pointer to the hook procedure.</param>
        /// <param name="dllHwnd">A handle to the DLL containing the hook procedure.</param>
        /// <param name="threadId">The identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>Handle to the hook procedure.</returns>
        [DllImport("User32.dll")][PreserveSig]
        public static extern IntPtr SetWindowsHookExA(int id, HotkeyHookManager.WinHookCallback callback, IntPtr dllHwnd, int threadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the <see cref="SetWindowsHookExA"/> function.
        /// </summary>
        /// <param name="hookPtr">A handle to the hook to be removed.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("User32.dll")][PreserveSig]
        public static extern bool UnhookWindowsHookEx(IntPtr hookPtr);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain.
        /// </summary>
        /// <param name="hook">This parameter is ignored.</param>
        /// <param name="nCode">The hook code passed to the current hook procedure.</param>
        /// <param name="wParam">The wParam value passed to the current hook procedure. </param>
        /// <param name="lParam">The lParam value passed to the current hook procedure.</param>
        /// <returns></returns>
        [DllImport("User32.dll")][PreserveSig]
        public static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wParam, ref IntPtr lParam);
    }
}
