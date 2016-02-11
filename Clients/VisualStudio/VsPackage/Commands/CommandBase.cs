using System;

namespace ToolsMenu.Commands
{
    abstract class CommandBase
    {
        protected readonly IServiceProvider ServiceProvider;

        protected CommandBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public abstract void OnClick(object sender, EventArgs e);
    }
}