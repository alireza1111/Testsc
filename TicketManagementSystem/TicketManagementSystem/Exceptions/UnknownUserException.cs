using System;

namespace TicketManagementSystem.Exceptions
{
    public class UnknownUserException : Exception
    {
        public UnknownUserException(string message) : base(message)
        {
        }
    }
}
