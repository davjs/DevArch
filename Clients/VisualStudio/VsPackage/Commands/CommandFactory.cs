using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace DevArch.Commands
{
    class CommandFactory
    {
        private readonly OleMenuCommandService _commandService;

        public CommandFactory(Package package)
        {
            _commandService = (package as IServiceProvider).GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (_commandService == null) throw new ServiceUnavailableException(typeof(IMenuCommandService));
        }

        // Adds our command handlers for menu (commands must exist in the command table file)
        public void AddCommand(CommandBase command,CommandID commandId)
        {
            _commandService.AddCommand(new MenuCommand(command.OnClick,commandId));
        }
    }
}