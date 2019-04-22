using System;

namespace Rulelx
{
    [Serializable]
    public class Message
    {
        public int type;
        public int model;
        public object data;
    }
}