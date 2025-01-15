using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CombatHeadgear
{
    public class HeadgearCommandExecutor
    {
        private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);
        private delegate IntPtr GetUiModuleDelegate(IntPtr basePtr);

        private ProcessChatBoxDelegate? processChatBoxDelegate;
        private IntPtr uiModule = IntPtr.Zero;

        public HeadgearCommandExecutor()
        {
            InitPointers();
        }

        private unsafe void InitPointers()
        {
            try
            {
                // Scan for the ProcessChatBox function
                processChatBoxDelegate = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(
                    Shared.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9"));

                // Scan for the UI module pointer
                var sigAddress = Shared.SigScanner.ScanText("49 8B DC 48 89 1D");
                IntPtr targetAddress = sigAddress + 10 + Marshal.ReadInt32(sigAddress + 6);

                var frameworkPtr = Marshal.ReadIntPtr(targetAddress);
                var getUiModulePtr = Shared.SigScanner.ScanText("E8 ?? ?? ?? ?? 80 7B 1D 01");

                var getUiModule = Marshal.GetDelegateForFunctionPointer<GetUiModuleDelegate>(getUiModulePtr);
                uiModule = getUiModule(frameworkPtr);
            }
            catch (Exception ex)
            {
                Shared.Log.Error($"Failed to initialize pointers: {ex.Message}");
            }
        }

        public async Task ExecuteHeadgearCommand(bool show)
        {
            if (processChatBoxDelegate == null || uiModule == IntPtr.Zero)
            {
                Shared.Log.Error("Unable to execute headgear and visor command: ProcessChatBox or uiModule is not initialized.");
                return;
            }

            // Apply Shared.Config settings
            if (Shared.Config.SetInverse)
            {
                show = !show;
            }

            if (Shared.Config.DelayMs > 0)
            {
                await Task.Delay(Shared.Config.DelayMs);
            }

            // Execute headgear and visor commands
            if (Shared.Config.ToggleHeadgear)
            {
                var headgearCommand = show ? "/displayhead on" : "/displayhead off";
                ExecuteCommand(headgearCommand);
            }

            if (Shared.Config.ToggleVisor)
            {
                var visorCommand = show ? "/visor on" : "/visor off";
                ExecuteCommand(visorCommand);
            }
        }

        private void ExecuteCommand(string command)
        {
            var commandBytes = System.Text.Encoding.UTF8.GetBytes(command);

            var memoryBlock1 = Marshal.AllocHGlobal(400);
            var memoryBlock2 = Marshal.AllocHGlobal(commandBytes.Length + 30);

            try
            {
                // Copy command bytes into allocated memory
                Marshal.Copy(commandBytes, 0, memoryBlock2, commandBytes.Length);
                Marshal.WriteByte(memoryBlock2 + commandBytes.Length, 0);
                Marshal.WriteInt64(memoryBlock1, memoryBlock2.ToInt64());
                Marshal.WriteInt64(memoryBlock1 + 8, 64);
                Marshal.WriteInt64(memoryBlock1 + 16, commandBytes.Length + 1);
                Marshal.WriteInt64(memoryBlock1 + 24, 0);

                // Execute the command
                processChatBoxDelegate?.Invoke(uiModule, memoryBlock1, IntPtr.Zero, 0);
            }
            finally
            {
                // Free allocated memory
                Marshal.FreeHGlobal(memoryBlock1);
                Marshal.FreeHGlobal(memoryBlock2);
            }
        }
    }
}
