﻿using System;

namespace Cesxhin.AnimeManga.Application.Exceptions
{
    public class ApiNotAuthorizeException : Exception
    {
        public ApiNotAuthorizeException() : base() { }
        public ApiNotAuthorizeException(string message) : base(message) { }
        public ApiNotAuthorizeException(string message, Exception inner) : base(message, inner) { }
    }
}
