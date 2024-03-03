using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Model.Enums {
    public enum Channels {
        KIMONO = 0,
        GRUPP = 1,
        ARCA = 2,
        FIDESIC = 3,
        UP = 4
    }
    public enum ChannelStatus {
        DEACTIVATED = -1,
        UNUSABLE = 0,
        ACTIVE = 1
    }
}
