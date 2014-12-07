using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WizardGame
{
    [Serializable]
    public enum ConnectionState
    {
        DISCONNECTED = 0,
        CONNECTED = 1,
        INACTIVE = 2
    }
}