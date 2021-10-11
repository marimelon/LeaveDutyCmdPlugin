using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace LeaveDutyCmdPlugin
{
    class LeaveDutyCmdPlugin : IDalamudPlugin
    {
        public string Name => "Leave Duty Cmd";

        private const string commandName = "/leaved";

        [PluginService]
        static DalamudPluginInterface Interface { get; set; }
        [PluginService]
        static CommandManager CommandManager { get; set; }
        [PluginService]
        static Framework Framework { get; set; }

        private delegate void LeaveDutyDelegate(char is_timeout);
        private LeaveDutyDelegate leaveDungeon;

        private readonly AddressResolver AddressResolver;

        private bool requesting = false;

        public LeaveDutyCmdPlugin()
        {
            AddressResolver = new AddressResolver();
            AddressResolver.Setup();

            this.leaveDungeon = Marshal.GetDelegateForFunctionPointer<LeaveDutyDelegate>(this.AddressResolver.LeaveDuty);

            Framework.Update += this.OnFrameworkUpdate;

            CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Immediately leave duty.(Do not display confirmation window)"
            });
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler(commandName);

            Framework.Update -= this.OnFrameworkUpdate;

            Interface.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            this.requesting = true;
        }

        private void OnFrameworkUpdate(Framework framework)
        {
            try
            {
                if (this.requesting)
                {
                    this.leaveDungeon.Invoke((char)0);
                    this.requesting = false;
                }
            }
            catch (Exception ex)
            {
                PluginLog.LogError(ex.Message);
            }
        }
    }
}
