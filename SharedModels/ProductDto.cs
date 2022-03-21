namespace SharedModels
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Category { get; set; }
        public int ItemsInStock { get; set; }
        public int ItemsReserved { get; set; }
    }
}
