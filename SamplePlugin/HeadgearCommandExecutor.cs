using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Logging;

namespace SamplePlugin
{
    public class HeadgearCommandExecutor
    {
        private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
        private delegate IntPtr GetUIModuleDelegate(IntPtr basePtr);

        private ProcessChatBoxDelegate? ProcessChatBox;
        private IntPtr uiModule = IntPtr.Zero;

        public HeadgearCommandExecutor(ISigScanner sigScanner)
        {
            InitializePointers(sigScanner);
        }

        private unsafe void InitializePointers(ISigScanner sigScanner)
        {
            try
            {
                var getUIModulePtr = sigScanner.ScanText("E8 ?? ?? ?? ?? 48 83 7F ?? 00 48 8B F0");
                var processChatBoxPtr = sigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
                var uiModulePtr = sigScanner.GetStaticAddressFromSig("48 8B 0D ?? ?? ?? ?? 48 8D 54 24 ?? 48 83 C1 10 E8");

                var getUIModule = Marshal.GetDelegateForFunctionPointer<GetUIModuleDelegate>(getUIModulePtr);
                uiModule = getUIModule(*(IntPtr*)uiModulePtr);
                ProcessChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(processChatBoxPtr);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to initialize pointers: {ex.Message}");
            }
        }

        public void ExecuteHeadgearCommand(bool show)
        {
            if (ProcessChatBox == null || uiModule == IntPtr.Zero)
            {
                PluginLog.Error("Unable to execute headgear command: ProcessChatBox or uiModule is not initialized.");
                return;
            }

            var command = show ? "/displayhead on" : "/displayhead off";
            var bytes = System.Text.Encoding.UTF8.GetBytes(command);

            var mem1 = Marshal.AllocHGlobal(400);
            var mem2 = Marshal.AllocHGlobal(bytes.Length + 30);

            try
            {
                Marshal.Copy(bytes, 0, mem2, bytes.Length);
                Marshal.WriteByte(mem2 + bytes.Length, 0);
                Marshal.WriteInt64(mem1, mem2.ToInt64());
                Marshal.WriteInt64(mem1 + 8, 64);
                Marshal.WriteInt64(mem1 + 8 + 8, bytes.Length + 1);
                Marshal.WriteInt64(mem1 + 8 + 8 + 8, 0);

                ProcessChatBox(uiModule, mem1, IntPtr.Zero, 0);
            }
            finally
            {
                Marshal.FreeHGlobal(mem1);
                Marshal.FreeHGlobal(mem2);
            }
        }
    }
}
