using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using System;
using System.Runtime.InteropServices;

namespace LeaveDutyCmd
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Leave Duty Cmd";

        private const string commandName = "/leaved";

        private delegate void LeaveDutyDelegate(char is_timeout);
        private LeaveDutyDelegate leaveDungeon;

        private DalamudPluginInterface pi;
        private AddressResolver resolver;

        private bool requesting = false;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;

            this.resolver = new AddressResolver();
            this.resolver.Setup(this.pi.TargetModuleScanner);

            this.leaveDungeon = Marshal.GetDelegateForFunctionPointer<LeaveDutyDelegate>(this.resolver.LeaveDuty);

            this.pi.Framework.OnUpdateEvent += this.OnFrameworkUpdate;

            this.pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Immediately leave duty.(Do not display confirmation window)"
            });
        }

        public void Dispose()
        {

            this.pi.CommandManager.RemoveHandler(commandName);

            this.pi.Framework.OnUpdateEvent -= this.OnFrameworkUpdate;

            this.pi.Dispose();
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
