namespace ClientTicketingSystem.CORE.Dtos;
public class TicketDto
{   
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;       
        public string AssignedEmpName { get; set; } = string.Empty;  
        public string ProductName { get; set; } = string.Empty; 
        public string Status { get; set; } = string.Empty;
        public bool IsFixed { get; set; }
    
}
