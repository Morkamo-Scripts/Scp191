using System.Collections.Generic;
using Exiled.API.Interfaces;
using Scp191.Variants;

namespace Scp191
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public Scp191ClassD Scp191ClassD { get; set; } = new();
        public Scp191Chaos Scp191Chaos { get; set; } = new();
        public Scp191Ntf Scp191Ntf { get; set; } = new();
        
        public HashSet<ItemType> NotAllowedItems { get; set; } =
        [
            ItemType.Adrenaline,
            ItemType.SCP207
        ];
    }
}