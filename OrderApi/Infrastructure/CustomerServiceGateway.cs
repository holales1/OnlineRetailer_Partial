using RestSharp;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public class CustomerServiceGateway : IServiceGateway<CustomerDto>
    {
        string customerServiceUrl;

        public CustomerServiceGateway(string url)
        {
            customerServiceUrl = url;
        }

        public CustomerDto Get(int id)
        {
            RestClient c = new RestClient(customerServiceUrl);

            var request = new RestRequest(id.ToString());
            var response = c.GetAsync<CustomerDto>(request);
            response.Wait();
            return response.Result;
        }

    }
}
