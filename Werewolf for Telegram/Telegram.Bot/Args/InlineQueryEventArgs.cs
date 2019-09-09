﻿using System;
using Telegram.Bot.Types;

namespace Telegram.Bot.Args
{
    public class InlineQueryEventArgs : EventArgs
    {
        public InlineQuery InlineQuery { get; private set; }

        internal InlineQueryEventArgs(Update update)
        {
            InlineQuery = update.InlineQuery;
        }

        internal InlineQueryEventArgs(InlineQuery inlineQuery)
        {
            InlineQuery = inlineQuery;
        }

        public static implicit operator InlineQueryEventArgs(UpdateEventArgs e) => new InlineQueryEventArgs(e.Update);
    }
}
