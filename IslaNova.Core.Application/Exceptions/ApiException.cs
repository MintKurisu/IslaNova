using System.Globalization;

namespace IslaNova.Core.Application.Exceptions
{
    public class ApiException : Exception
    {
        public int StatusCode { get; set; }
        public ApiException() : base() { }

        public ApiException(string message) : base(message) { }

        public ApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        { }

    }
}
