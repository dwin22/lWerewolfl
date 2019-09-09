﻿using System;
using Telegram.Bot.Types;

namespace Telegram.Bot.Args
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; private set; }

        internal MessageEventArgs(Update update)
        {
            Message = update.Message;
        }

        internal MessageEventArgs(Message message)
        {
            Message = message;
        }

        public static implicit operator MessageEventArgs(UpdateEventArgs e) => new MessageEventArgs(e.Update);
    }
}
