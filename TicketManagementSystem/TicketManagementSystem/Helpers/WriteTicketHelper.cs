using System.IO;
using System.Text.Json;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Helpers
{
    public class WriteTicketHelper
    {
        public static void WriteTicketToFile(Ticket ticket)
        {
            var ticketJson = JsonSerializer.Serialize(ticket);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"ticket_{ticket.Id}.json"), ticketJson);
        }
    }
}
