namespace BlackCode.Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int? Codigo { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId) && Codigo == null;
    }
}
