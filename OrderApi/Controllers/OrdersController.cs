using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repositoryOrders;
        private readonly IRepositoryOrderLine<OrderLine> repositoryOrderLines;

        public OrdersController(IRepository<Order> repos, IRepositoryOrderLine<OrderLine> repos2)
        {
            repositoryOrders = repos;
            repositoryOrderLines = repos2;
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repositoryOrders.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repositoryOrders.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // GET orders/customer/5
        [HttpGet("customer/{id}")]
        public IEnumerable<Order> GetAllByCustomerId(int id)
        {
            ObservableCollection<Order> orders = new ObservableCollection<Order>(repositoryOrders.GetAll());
            var nullItems = orders.Where(p => p.CustomerId != id).ToList();
            foreach (var item in nullItems)
            {
                orders.Remove(item);
            }
            return orders;
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            RestClient cCustomer = new RestClient("https://localhost:5011/customers/");
            var requestCustomer = new RestRequest(order.CustomerId.ToString());
            var responseCustomer = cCustomer.GetAsync<Customer>(requestCustomer);
            responseCustomer.Wait();
            var orderedCustomer = responseCustomer.Result;

            if (orderedCustomer.Id == 0 )
            {
                string content = "You dont have an account, please register before order.";
                sendEmail(orderedCustomer.Email, content);
                return BadRequest();
            }

            if (orderedCustomer.CreditStanding != 0)
            {
                string content = String.Format("You already have a deb of {0}kr, please pay it before order more.", orderedCustomer.CreditStanding);
                sendEmail(orderedCustomer.Email, content);
                return BadRequest();
            }

            // Call ProductApi to get the product ordered
            // You may need to change the port number in the BaseUrl below
            // before you can run the request.
            RestClient c = new RestClient("https://localhost:5001/products/");
            var request = new RestRequest(order.ProductId.ToString());
            var response = c.GetAsync<Product>(request);
            response.Wait();
            var orderedProduct = response.Result;

            if (order.Quantity <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
            {
                // reduce the number of items in stock for the ordered product,
                // and create a new order.
                orderedProduct.ItemsReserved += order.Quantity;
                var updateRequest = new RestRequest(orderedProduct.Id.ToString());
                updateRequest.AddJsonBody(orderedProduct);
                var updateResponse = c.PutAsync(updateRequest);
                updateResponse.Wait();

                orderedCustomer.CreditStanding += (int)orderedProduct.Price * order.Quantity;

                var updateRequestCustomer = new RestRequest(orderedCustomer.Id.ToString());
                updateRequestCustomer.AddJsonBody(orderedCustomer);
                var updateResponseCustomer = cCustomer.PutAsync(updateRequestCustomer);
                updateResponseCustomer.Wait();

                if (updateResponse.IsCompletedSuccessfully)
                {
                    order.CustomerId = orderedCustomer.Id;
                    order.State = 0;
                    var newOrder = repositoryOrders.Add(order);
                    string content = "The order has been accepted.";
                    sendEmail(orderedCustomer.Email, content);
                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
                }
            }
            else
            {
                string content = "We haven't enough items today, please do the order another day.";
                sendEmail(orderedCustomer.Email, content);
            }

            // If the order could not be created, "return no content".
            return NoContent();
        }

        // Put orders/paid/5
        [HttpPut("paid/{id}")]
        public IActionResult PaidOrder(int id)
        {
            Order order = repositoryOrders.Get(id);

            if (order.State == Order.Status.Shipped)
            {
                RestClient c = new RestClient("https://localhost:5011/customers/");
                var request = new RestRequest(order.CustomerId.ToString());
                var response = c.GetAsync<Customer>(request);
                response.Wait();
                var customer = response.Result;

                customer.CreditStanding = 0;

                var updateRequest = new RestRequest(customer.Id.ToString());
                updateRequest.AddJsonBody(customer);
                var updateResponse = c.PutAsync(updateRequest);
                updateResponse.Wait();
                if (updateResponse.IsCompletedSuccessfully)
                {
                    order.State = Order.Status.Paid;
                    repositoryOrders.Edit(order);
                    return new NoContentResult();
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }

        // Put orders/cancel/5
        [HttpPut("cancel/{id}")]
        public IActionResult CancelOrder(int id)
        {
            Order order = repositoryOrders.Get(id);

            if (order.State == Order.Status.Completed)
            {
                RestClient c = new RestClient("https://localhost:5001/products/");
                var request = new RestRequest(order.ProductId.ToString());
                var response = c.GetAsync<Product>(request);
                response.Wait();
                var product = response.Result;

                product.ItemsReserved -= order.Quantity;

                var updateRequest = new RestRequest(product.Id.ToString());
                updateRequest.AddJsonBody(product);
                var updateResponse = c.PutAsync(updateRequest);
                updateResponse.Wait();
                if (updateResponse.IsCompletedSuccessfully)
                {
                    order.State = Order.Status.Cancelled;
                    repositoryOrders.Edit(order);
                    return new NoContentResult();
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }

        // Put orders/send/5
        [HttpPut("send/{id}")]
        public IActionResult SendOrder(int id)
        {
            Order order = repositoryOrders.Get(id);

            if (order.State == Order.Status.Completed)
            {
                RestClient clientProduct = new RestClient("https://localhost:5001/products/");
                var requestProduct = new RestRequest(order.ProductId.ToString());
                var responseProduct = clientProduct.GetAsync<Product>(requestProduct);
                responseProduct.Wait();
                var product = responseProduct.Result;

                product.ItemsReserved -= order.Quantity;
                product.ItemsInStock -= order.Quantity;

                var updateRequest = new RestRequest(product.Id.ToString());
                updateRequest.AddJsonBody(product);
                var updateResponse = clientProduct.PutAsync(updateRequest);
                updateResponse.Wait();
                if (updateResponse.IsCompletedSuccessfully)
                {
                    order.State = Order.Status.Shipped;
                    repositoryOrders.Edit(order);

                    

                    RestClient clientCustomer = new RestClient("https://localhost:5011/customers/");
                    var requestCustomer = new RestRequest(order.CustomerId.ToString());
                    var responseCustomer = clientCustomer.GetAsync<Customer>(requestCustomer);
                    responseCustomer.Wait();
                    Customer customer = responseCustomer.Result;

                    string content = String.Format("Order with id: {0}, has been shipped.", order.Id.ToString());
                    bool sended = sendEmail(customer.Email, content);

                    if (!sended)
                    {
                        //Add wait list to send the email later...
                    }

                    return new NoContentResult();
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }

        private bool sendEmail(string dest, string content)
        {

            RestClient clientTotalEmail = new RestClient("https://localhost:5021/emails/total/");
            var requestTotalEmail = new RestRequest();
            var responseTotalEmail = clientTotalEmail.GetAsync<int>(requestTotalEmail);
            responseTotalEmail.Wait();
            int totalEmails = responseTotalEmail.Result;

            Email email = new Email();
            email.Id = totalEmails + 1;
            email.Destination = dest;
            email.Content = content;

            RestClient clientSendEmail = new RestClient("https://localhost:5021/emails/");
            var requestSendEmail = new RestRequest();
            requestSendEmail.AddJsonBody(email);
            var responseSendEmail = clientSendEmail.PostAsync(requestSendEmail);
            responseSendEmail.Wait();

            return responseSendEmail.IsCompletedSuccessfully;

        }

    }
}

