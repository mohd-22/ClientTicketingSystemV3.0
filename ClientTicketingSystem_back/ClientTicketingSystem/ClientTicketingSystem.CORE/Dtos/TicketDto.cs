namespace ClientTicketingSystem.CORE.Dtos;
public class TicketDto
{
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;       
        public string AssignedEmpName { get; set; } = string.Empty;  
        public string ProductModuleName { get; set; } = string.Empty; 
        public string Status { get; set; } = string.Empty;
        public bool IsFixed { get; set; }
    
}
