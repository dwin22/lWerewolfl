﻿using System;
using Telegram.Bot.Types;

namespace Telegram.Bot.Args
{
    public class CallbackQueryEventArgs : EventArgs
    {
        public CallbackQuery CallbackQuery { get; private set; }

        internal CallbackQueryEventArgs(Update update)
        {
            CallbackQuery = update.CallbackQuery;
        }

        internal CallbackQueryEventArgs(CallbackQuery callbackQuery)
        {
            CallbackQuery = callbackQuery;
        }

        public static implicit operator CallbackQueryEventArgs(UpdateEventArgs e) => new CallbackQueryEventArgs(e.Update);
    }
}
