namespace Rezqa.Domain.Entities
{
    public class FieldOption
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!; // "BMW M5"
        public string Value { get; set; } = null!; // "m5"


        public int DynamicFieldId { get; set; }
        public DynamicField DynamicField { get; set; }


        public string? ParentValue { get; set; } // value of parent field (like "bmw")
    }
}