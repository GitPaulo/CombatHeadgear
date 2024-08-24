using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Plugin.Services;

namespace CombatHeadgear
{
    public class HeadgearCommandExecutor
    {
        private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
        private delegate IntPtr GetUiModuleDelegate(IntPtr basePtr);

        private ProcessChatBoxDelegate? _processChatBox;
        private IntPtr _uiModule = IntPtr.Zero;
        private IPluginLog _pluginLog;

        public HeadgearCommandExecutor(ISigScanner sigScanner, IPluginLog pluginLog)
        {
            _pluginLog = pluginLog;
            InitializePointers(sigScanner);
        }

        private unsafe void InitializePointers(ISigScanner sigScanner)
        {
            try
            {
                // Updated signatures
                _processChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(
                    sigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));

                var sigAddress = sigScanner.ScanText("49 8B DC 48 89 1D");
                IntPtr targetAddress = sigAddress + 10 + Marshal.ReadInt32(sigAddress + 6);

                var frameworkPtr = Marshal.ReadIntPtr(targetAddress);
                var getUiModulePtr = sigScanner.ScanText("E8 ?? ?? ?? ?? 80 7B 1D 01");

                var getUiModule = Marshal.GetDelegateForFunctionPointer<GetUiModuleDelegate>(getUiModulePtr);
                _uiModule = getUiModule(frameworkPtr);
            }
            catch (Exception ex)
            {
                _pluginLog.Error($"Failed to initialize pointers: {ex.Message}");
            }
        }

        public async Task ExecuteHeadgearCommand(bool show, Configuration configuration)
        {
            if (_processChatBox == null || _uiModule == IntPtr.Zero)
            {
                _pluginLog.Error("Unable to execute headgear and visor command: ProcessChatBox or uiModule is not initialized.");
                return;
            }

            if (configuration.SetInverse)
            {
                show = !show;
            }

            if (configuration.DelayMs > 0)
            {
                await Task.Delay(configuration.DelayMs);
            }

            if (configuration.ToggleHeadgear)
            {
                var headgearCommand = show ? "/displayhead on" : "/displayhead off";
                ExecuteCommand(headgearCommand);
            }

            if (configuration.ToggleVisor)
            {
                var visorCommand = show ? "/visor on" : "/visor off";
                ExecuteCommand(visorCommand);
            }
        }

        private void ExecuteCommand(string command)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(command);

            var mem1 = Marshal.AllocHGlobal(400);
            var mem2 = Marshal.AllocHGlobal(bytes.Length + 30);

            try
            {
                Marshal.Copy(bytes, 0, mem2, bytes.Length);
                Marshal.WriteByte(mem2 + bytes.Length, 0);
                Marshal.WriteInt64(mem1, mem2.ToInt64());
                Marshal.WriteInt64(mem1 + 8, 64);
                Marshal.WriteInt64(mem1 + 16, bytes.Length + 1);
                Marshal.WriteInt64(mem1 + 24, 0);

                _processChatBox?.Invoke(_uiModule, mem1, IntPtr.Zero, 0);
            }
            finally
            {
                Marshal.FreeHGlobal(mem1);
                Marshal.FreeHGlobal(mem2);
            }
        }
    }
}
