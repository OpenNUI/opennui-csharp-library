using System;
using System.Collections.Generic;
using System.Text;

namespace OpenNUI.CSharp.Library
{
    enum STCHeader : short
    {
        REQUEST_PLATFORM_TYPE = 0x0001,
        REQUEST_PIPE_CONNECTION = 0x0002,
        TEST_TEXTMESSAGE = 0x0003,
        RESPONSE_COLOR_FRAME = 0x0004,
        RESPONSE_DEPTH_FRAME = 0x0005,
        RESPONSE_BODY_FRAME = 0x0006,
        SEND_ALL_SENSOR_INFO = 0x0007,
        SEND_NEW_SENSOR_INFO = 0x0008,
        SEND_CHANGED_SENSOR_INFO = 0x0009,
        SEND_TRIGGER_EVENT_DATA = 0x0010,
    }
    enum CTSHeader : short
    {
        HELLO_ACK = 0x0001,
        READY_CONNECTION_ACK = 0x0002,
        TEST_TEXTMESSAGE = 0x0003,

        REQUEST_COLOR_FRAME = 0x0004,
        REQUEST_DEPTH_FRAME = 0x0005,
        REQUEST_BODY_FRAME = 0x0006,

        REQUEST_SENSOR_DATA = 0x0007,
    }
}
