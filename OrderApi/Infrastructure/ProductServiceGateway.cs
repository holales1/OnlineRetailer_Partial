using RestSharp;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public class ProductServiceGateway : IServiceGateway<ProductDto>
    {
        string productServiceUrl;

        public ProductServiceGateway(string url)
        {
            productServiceUrl = url;
        }

        public ProductDto Get(int id)
        {
            RestClient c = new RestClient(productServiceUrl);

            var request = new RestRequest(id.ToString());
            var response = c.GetAsync<ProductDto>(request);
            response.Wait();
            return response.Result;
        }

    }
}
