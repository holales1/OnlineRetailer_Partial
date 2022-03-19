﻿using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using RestSharp;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repositoryOrders;
        //private readonly IRepositoryOrderLine<OrderLine> repositoryOrderLines;
        private readonly IServiceGateway<ProductDto> productGateway;
        private readonly IServiceGateway<CustomerDto> customerGateway;
        private readonly IServiceGateway<EmailDto> emailGateway;
        private readonly IMessagePublisher messagePublisher;

        public OrdersController(IRepository<Order> repos,
                                IRepositoryOrderLine<OrderLine> repos2,
                                IServiceGateway<ProductDto> productSGateway,
                                IServiceGateway<CustomerDto> customerSGateway,
                                IServiceGateway<EmailDto> emailSGateway,
                                IMessagePublisher publisher)
        {
            repositoryOrders = repos;
            productGateway = productSGateway;
            customerGateway = customerSGateway;
            emailGateway = emailSGateway;
            messagePublisher = publisher;
            //repositoryOrderLines = repos2;
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

        // GET orders/customer/1
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
        public IActionResult Post([FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            CustomerDto customerDto = getCustomer(order.CustomerId);

            if (customerDto.Id == 0)
            {
                string content = "You dont have an account, please register before order.";
                sendEmail(customerDto.Email, content);

                return StatusCode(403, "The customer doesn't exists");
            }

            if (customerDto.CreditStanding != 0)
            {
                string content = String.Format("You already have a deb of {0}kr, please pay it before order more.", customerDto.CreditStanding);
                sendEmail(customerDto.Email, content);

                return StatusCode(403, "The customer has debts");
            }

            List<ProductDto> productDtoList;

            if (!checkIfAllProductsAreAvaliable(order, out productDtoList))
            {
                string content = "We haven't enough items today, please do the order another day.";
                sendEmail(customerDto.Email, content);

                return StatusCode(500, "Not enough items in stock.");
            }

            try
            {
                messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId,
                                                                  order.OrderLines,
                                                                  calculateAmount(order, productDtoList),
                                                                  "completed");

                order.State = Order.Status.Completed;
                var newOrder = repositoryOrders.Add(order);

                string content = "The order has been accepted.";
                sendEmail(customerDto.Email, content);

                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }
            catch
            {
                return StatusCode(500, "An error happened. Try again.");
            }
        }

        // Put orders/pay/5
        [HttpPut("pay/{id}")]
        public IActionResult PayOrder(int id)
        {
            Order order = repositoryOrders.Get(id);

            if (order.State == Order.Status.Shipped)
            {
                CustomerDto customerDto = getCustomer(order.CustomerId);

                try
                {
                    messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId,
                                                                      order.OrderLines,
                                                                      calculateAmount(order),
                                                                      "paid");

                    order.State = Order.Status.Paid;
                    repositoryOrders.Edit(order);

                    string content = String.Format("Thanks for pay your order {0}", order.Id);
                    sendEmail(customerDto.Email, content);

                    return StatusCode(201, "Order paid correctly");
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                return StatusCode(401, "You couldn't pay this order");
            }
        }

        // Put orders/cancel/5
        [HttpPut("cancel/{id}")]
        public IActionResult CancelOrder(int id)
        {
            Order order = repositoryOrders.Get(id);

            if (order.State == Order.Status.Completed)
            {
                CustomerDto customerDto = getCustomer(order.CustomerId);

                try
                {
                    messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId,
                                                                      order.OrderLines,
                                                                      calculateAmount(order),
                                                                      "cancelled");

                    order.State = Order.Status.Cancelled;
                    repositoryOrders.Edit(order);

                    string content = String.Format("The order with id: {0}, has been cancelled.", order.Id);
                    sendEmail(customerDto.Email, content);

                    return StatusCode(201, "Order cancelled correctly");
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                return StatusCode(401, "You couldn't cancel this order");
            }
        }

        // Put orders/send/5
        [HttpPut("send/{id}")]
        public IActionResult SendOrder(int id)
        {
            Order order = repositoryOrders.Get(id);

            if (order.State == Order.Status.Completed)
            {
                CustomerDto customerDto = getCustomer(order.CustomerId);

                try
                {
                    messagePublisher.PublishOrderStatusChangedMessage(order.CustomerId,
                                                                      order.OrderLines,
                                                                      "sent");

                    order.State = Order.Status.Shipped;
                    repositoryOrders.Edit(order);

                    string content = String.Format("Order with id: {0}, has been shipped.", order.Id.ToString());
                    sendEmail(customerDto.Email, content);

                    return StatusCode(201, "Order cancelled correctly");
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                return StatusCode(401, "It is impossible send this order");
            }
        }



        public bool sendEmail(string dest, string content)
        {
            RestClient clientTotalEmail = new RestClient("http://emailapi/emails/total/");
            var requestTotalEmail = new RestRequest();
            var responseTotalEmail = clientTotalEmail.GetAsync<int>(requestTotalEmail);
            responseTotalEmail.Wait();
            int totalEmails = responseTotalEmail.Result;

            EmailDto emailDto = new EmailDto();
            emailDto.Id = totalEmails + 1;
            emailDto.Destination = dest;
            emailDto.Content = content;

            RestClient clientSendEmail = new RestClient("http://emailapi/emails/");
            var requestSendEmail = new RestRequest();
            requestSendEmail.AddJsonBody(emailDto);
            var responseSendEmail = clientSendEmail.PostAsync(requestSendEmail);
            responseSendEmail.Wait();
            return responseSendEmail.IsCompletedSuccessfully;

        }

        public ProductDto getProduct(int productId)
        {
            RestClient clientProduct = new RestClient("http://productapi/products/");
            var requestProduct = new RestRequest(productId.ToString());
            var responseProduct = clientProduct.GetAsync<ProductDto>(requestProduct);
            responseProduct.Wait();
            ProductDto productDto = responseProduct.Result;

            return productDto;
        }

        public CustomerDto getCustomer(int customerId)
        {
            return customerGateway.Get(customerId);
        }

        public List<ProductDto> getProductList(Order order)
        {
            List<ProductDto> productDtoList = new List<ProductDto>();
            foreach (var orderLine in order.OrderLines)
            {
                ProductDto productDto = getProduct(orderLine.ProductId);
                productDtoList.Add(productDto);
            }
            return productDtoList;
        }

        public bool checkIfAllProductsAreAvaliable(Order order, out List<ProductDto> productDtoList)
        {
            productDtoList = getProductList(order);
            foreach (var productDto in productDtoList)
            {
                var selectedOrder = order.OrderLines.Where(o => o.ProductId == productDto.Id).FirstOrDefault();
                if (selectedOrder.Quantity > productDto.ItemsInStock - productDto.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }

        public int calculateAmount(Order order, List<ProductDto> products)
        {
            int total = 0;
            foreach (OrderLine line in order.OrderLines)
            {
                ProductDto productDto = products.Find(x => x.Id == line.Id);
                total += line.Quantity * productDto.Price;
            }

            return total;
        }

        public int calculateAmount(Order order)
        {
            int total = 0;
            List<ProductDto> products = getProductList(order);
            foreach (OrderLine line in order.OrderLines)
            {
                ProductDto productDto = products.Find(x => x.Id == line.Id);
                total += line.Quantity * productDto.Price;
            }

            return total;
        }

    }
}

