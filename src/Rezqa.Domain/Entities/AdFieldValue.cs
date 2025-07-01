namespace Rezqa.Domain.Entities
{
    public class AdFieldValue
    {
        public int Id { get; set; }

        public int AdId { get; set; }
        public Ad Ad { get; set; }

        public int DynamicFieldId { get; set; }
        public DynamicField DynamicField { get; set; }

        public string? Value { get; set; }
    }
}