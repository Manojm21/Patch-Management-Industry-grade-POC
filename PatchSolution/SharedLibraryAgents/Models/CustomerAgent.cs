using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibraryAgents.Models
{
    public class CustomerAgent
{
    public int Id { get; set; }
    public string AgentId { get; set; }
    public string CustomerName { get; set; }
    public string CurrentVersion { get; set; }

   
}

}
//dotnet ef migrations add AddNewTable
//dotnet ef database update

