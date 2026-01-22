using Exiled.API.Interfaces;

namespace Scp191
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public Scp191Handler Scp191 { get; set; } = new();
    }
}