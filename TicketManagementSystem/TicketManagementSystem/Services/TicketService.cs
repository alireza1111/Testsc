using System;
using EmailService;
using TicketManagementSystem.Exceptions;
using TicketManagementSystem.Helpers;
using TicketManagementSystem.Models;
using TicketManagementSystem.Repositories;

namespace TicketManagementSystem
{
    public class TicketService
    {
        private readonly UserRepository userRepository;
        private readonly EmailServiceProxy emailService;
        public TicketService()
        {
            userRepository = new UserRepository();
            emailService = new EmailServiceProxy();
        }
        public int CreateTicket(string title, Priority priority, string assignedTo, string description, DateTime createdTime, bool isPayingCustomer)
        {
            ValidateTicketInput(title, description);

            var user = userRepository.GetUser(assignedTo);

            ValidateUser(user, assignedTo);

            PrioritiseBasedOnCreationTime(priority, createdTime);
            PrioritiseBasedOnTitle(priority, title);

            EmailHighPriorityTicket(priority, title, assignedTo);

            double price = CalculateTicketPrice(isPayingCustomer, priority);

            var ticket = new Ticket
            {
                Title = title,
                AssignedUser = user,
                Priority = priority,
                Description = description,
                Created = createdTime,
                PriceDollars = price,
                AccountManager = isPayingCustomer ? userRepository.GetAccountManager() : null
            };

            WriteTicketHelper.WriteTicketToFile(ticket); //TODO: INFO_Alireza_2023-06-28 Shall we keep the method in a helper class?

            return TicketRepository.CreateTicket(ticket);
        }

        private double CalculateTicketPrice(bool isPayingCustomer, Priority priority)
        {
            return isPayingCustomer ? priority == Priority.High ? 100 : 50 : 0;
        }

        private void EmailHighPriorityTicket(Priority priority, string title, string assignedTo)
        {
            if (priority == Priority.High)
            {
                emailService.SendEmailToAdministrator(title, assignedTo);
            }
        }

        private Priority PrioritiseBasedOnTitle(Priority currentPriority, string title)
        {
            if ((title.Contains("Crash") || title.Contains("Important") || title.Contains("Failure")) && currentPriority != Priority.High)
            {
                if (currentPriority == Priority.Low)
                {
                    return Priority.Medium;
                }
                else if (currentPriority == Priority.Medium)
                {
                    return Priority.High;
                }
            }
            return currentPriority;
        }

        private Priority PrioritiseBasedOnCreationTime(Priority priority, DateTime createdTime)
        {
            if (DateTime.UtcNow - createdTime > TimeSpan.FromHours(1))
            {
                if (priority == Priority.Low)
                {
                    return Priority.Medium;
                }
                else if (priority == Priority.Medium)
                {
                    return Priority.High;
                }
            }

            return priority;
        }

        private void ValidateUser(User user, string assignedTo)
        {
            if (user == null)
            {
                throw new UnknownUserException("User " + assignedTo + " not found");
            }
        }

        private void ValidateTicketInput(string title, string description)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                throw new InvalidTicketException("Title or Description should not be null");
            }
        }

        public void AssignTicket(int id, string username)
        {
            var user = userRepository.GetUser(username);
            ValidateUser(user, username);

            var ticket = TicketRepository.GetTicket(id);
            ValidateTicket(ticket, id);

            ticket.AssignedUser = user;
            TicketRepository.UpdateTicket(ticket);
        }

        private void ValidateTicket(Ticket ticket, int id)
        {
            if (ticket == null)
            {
                throw new ApplicationException("No ticket found for id " + id);
            }
        }
    }
}
