using System;

namespace Cesxhin.AnimeManga.Application.Exceptions
{
    public class ApiGenericException : Exception
    {
        public ApiGenericException() : base() { }
        public ApiGenericException(string message) : base(message) { }
        public ApiGenericException(string message, Exception inner) : base(message, inner) { }
    }
}
