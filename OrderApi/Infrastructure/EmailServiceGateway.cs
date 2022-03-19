using RestSharp;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public class EmailServiceGateway : IServiceGateway<EmailDto>
    {
        string emailServiceUrl;

        public EmailServiceGateway(string url)
        {
            emailServiceUrl = url;
        }

        public EmailDto Get(int id)
        {
            RestClient c = new RestClient(emailServiceUrl);

            var request = new RestRequest(id.ToString());
            var response = c.GetAsync<EmailDto>(request);
            response.Wait();
            return response.Result;
        }

    }
}
