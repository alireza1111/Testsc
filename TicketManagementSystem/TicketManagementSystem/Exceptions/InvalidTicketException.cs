using System;

namespace TicketManagementSystem.Exceptions
{
    public class InvalidTicketException : Exception
    {
        public InvalidTicketException(string message) : base(message)
        {
        }
    }
}
