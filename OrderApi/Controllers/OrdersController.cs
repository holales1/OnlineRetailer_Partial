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

            Customer customer = getCustomer(order.CustomerId);

            if (customer.Id == 0 )
            {
                string content = "You dont have an account, please register before order.";
                sendEmail(customer.Email, content);
                return BadRequest();
            }

            if (customer.CreditStanding != 0)
            {
                string content = String.Format("You already have a deb of {0}kr, please pay it before order more.", customer.CreditStanding);
                sendEmail(customer.Email, content);
                return BadRequest();
            }

            Product product = getProduct(order.ProductId);

            if (order.Quantity <= product.ItemsInStock - product.ItemsReserved)
            {
                product.ItemsReserved += order.Quantity;
                bool productUpdated = setProduct(product);

                customer.CreditStanding += (int)product.Price * order.Quantity;

                setCustomer(customer);

                if (productUpdated)
                {
                    order.CustomerId = customer.Id;
                    order.State = 0;
                    var newOrder = repositoryOrders.Add(order);

                    string content = "The order has been accepted.";
                    sendEmail(customer.Email, content);

                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
                }
            }
            else
            {
                string content = "We haven't enough items today, please do the order another day.";
                sendEmail(customer.Email, content);
            }

            // If the order could not be created, "return no content".
            return NoContent();
        }

        // Put orders/paid/5
        [HttpPut("pay/{id}")]
        public IActionResult PayOrder(int id)
        {
            Order order = repositoryOrders.Get(id);

            if (order.State == Order.Status.Shipped)
            {

                Customer customer = getCustomer(order.CustomerId);

                customer.CreditStanding -= totalAmount(order);

                bool customerUpdate = setCustomer(customer);
                if (customerUpdate)
                {
                    order.State = Order.Status.Paid;
                    repositoryOrders.Edit(order);

                    string content = String.Format("Thanks for pay your order {0}", order.Id);
                    sendEmail(customer.Email, content);

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
                bool productsUpdated= cancelProductStock(order);

                if (productsUpdated)
                {
                    order.State = Order.Status.Cancelled;
                    repositoryOrders.Edit(order);

                    Customer customer = getCustomer(order.CustomerId);
                    customer.CreditStanding -= totalAmount(order);
                    setCustomer(customer);

                    string content = String.Format("The order with id: {0}, has been cancelled.", order.Id);
                    sendEmail(customer.Email, content);

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
                bool productsUpdated = sendProductStock(order);

                if (productsUpdated)
                {
                    order.State = Order.Status.Shipped;
                    repositoryOrders.Edit(order);

                    Customer customer = getCustomer(order.CustomerId);

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

        private Product getProduct(int productId)
        {
            RestClient clientProduct = new RestClient("https://localhost:5001/products/");
            var requestProduct = new RestRequest(productId.ToString());
            var responseProduct = clientProduct.GetAsync<Product>(requestProduct);
            responseProduct.Wait();
            Product product = responseProduct.Result;

            return product;
        }

        private bool setProduct(Product product)
        {
            RestClient clientProduct = new RestClient("https://localhost:5001/products/");

            var updateRequest = new RestRequest(product.Id.ToString());
            updateRequest.AddJsonBody(product);
            var updateResponse = clientProduct.PutAsync(updateRequest);
            updateResponse.Wait();

            return updateResponse.IsCompletedSuccessfully;
        }

        private bool setCustomer(Customer customer)
        {
            RestClient clientCustomer = new RestClient("https://localhost:5011/customers/");
            var requestCustomer = new RestRequest(customer.Id.ToString());
            requestCustomer.AddJsonBody(customer);
            var responseCustomer = clientCustomer.PutAsync(requestCustomer);
            responseCustomer.Wait();

            return responseCustomer.IsCompletedSuccessfully;
        }
    
        private Customer getCustomer(int customerId)
        {
            RestClient cCustomer = new RestClient("https://localhost:5011/customers/");
            var requestCustomer = new RestRequest(customerId.ToString());
            var responseCustomer = cCustomer.GetAsync<Customer>(requestCustomer);
            responseCustomer.Wait();
            var orderedCustomer = responseCustomer.Result;

            return orderedCustomer;
        }
    
        private bool sendProductStock(Order order)
        {
            bool aux = true;
            foreach(OrderLine line in order.orderLines)
            {
                Product product = getProduct(line.ProductId);

                product.ItemsReserved -= line.Quantity;
                product.ItemsInStock -= line.Quantity;

                aux = setProduct(product);
            }
            return aux;
        }

        private bool cancelProductStock(Order order)
        {
            bool aux = true;
            foreach (OrderLine line in order.orderLines)
            {
                Product product = getProduct(line.ProductId);

                product.ItemsReserved -= line.Quantity;

                aux = setProduct(product);
            }
            return aux;
        }

        private int totalAmount(Order order)
        {
            int total = 0;
            foreach (OrderLine line in order.orderLines)
            {
                Product product = getProduct(line.ProductId);
                total += line.Quantity * product.Price;
            }            
            return total;
        }


    }
}

