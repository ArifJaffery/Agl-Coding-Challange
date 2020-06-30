
namespace InfraStructure.Connectors.Models
{
    public class ErrorModel
    {
        public string Code { get; set; }
        public string CorrelationId { get; set; }
        public string Target { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public ErrorDetailModel[] Details { get; set; }

    }
}
