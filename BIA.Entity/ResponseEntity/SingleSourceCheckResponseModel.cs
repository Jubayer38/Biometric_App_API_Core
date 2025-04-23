namespace BIA.Entity.ResponseEntity
{
    public class SingleSourceCheckResponseModel
    {
        public int Status { get; set; }
        public string Message { get; set; }
    }

    public class SingleSourceCheckResponseModelRevamp
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
