using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Models
{
    public class ProductConverter : IConverter<Product, ProductDto>
    {
        public Product Convert(ProductDto sharedProduct)
        {
            return new Product
            {
                Id = sharedProduct.Id,
                Name = sharedProduct.Name,
                Price = sharedProduct.Price,
                Category = sharedProduct.Category,
                ItemsInStock = sharedProduct.ItemsInStock,
                ItemsReserved = sharedProduct.ItemsReserved
            };
        }

        public ProductDto Convert(Product hiddenProduct)
        {
            return new ProductDto
            {
                Id = hiddenProduct.Id,
                Name = hiddenProduct.Name,
                Price = hiddenProduct.Price,
                Category = hiddenProduct.Category,
                ItemsInStock = hiddenProduct.ItemsInStock,
                ItemsReserved = hiddenProduct.ItemsReserved
            };
        }

        public IEnumerable<Product> ConvertList(IEnumerable<ProductDto> modelList)
        {
            List<Product> productList = new List<Product>();

            foreach (var productDto in modelList.ToList())
            {
                Product product = Convert(productDto);
                productList.Add(product);
            }

            return productList;
        }

        public IEnumerable<ProductDto> ConvertList(IEnumerable<Product> modelList)
        {
            List<ProductDto> productDtoList = new List<ProductDto>();

            foreach (var product in modelList.ToList())
            {
                ProductDto productDto = Convert(product);
                productDtoList.Add(productDto);
            }

            return productDtoList;
        }

    }
}
