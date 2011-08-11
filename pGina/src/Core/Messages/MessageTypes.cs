using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Core.Messages
{
    public enum MessageType
    {
        Unknown         = 0x00,
        Hello           = 0x01,
        Disconnect      = 0x02,
        Ack             = 0x03,
        Log             = 0x04,
        LoginRequest    = 0x05,
        LoginResponse   = 0x06,
    }
}
