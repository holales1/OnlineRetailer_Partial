﻿using System;
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
        private readonly IRepository<Order> repository;

        public OrdersController(IRepository<Order> repos)
        {
            repository = repos;
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
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
            ObservableCollection<Order> orders = new ObservableCollection<Order>(repository.GetAll());
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

            if (orderedCustomer.Id == 0 || orderedCustomer.CreditStanding != 0)
            {
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

                if (updateResponse.IsCompletedSuccessfully)
                {
                    order.CustomerId = orderedCustomer.Id;
                    order.State = 0;
                    var newOrder = repository.Add(order);
                    return CreatedAtRoute("GetOrder",
                        new { id = newOrder.Id }, newOrder);
                }
            }

            // If the order could not be created, "return no content".
            return NoContent();
        }

        // PUT orders/5
        [HttpPut("{id}")]
        public IActionResult PutState(int id, [FromBody] Order order)
        {
            if (order == null || order.Id != id)
            {
                return BadRequest();
            }

            var modifiedOrder = repository.Get(id);

            if (modifiedOrder == null)
            {
                return NotFound();
            }

            modifiedOrder.State = order.State;

            repository.Edit(modifiedOrder);
            return new NoContentResult();
        }

        // Put orders/cancel/5
        [HttpPut("cancel/{id}")]
        public IActionResult CancelOrder(int id)
        {
            Order order = repository.Get(id);

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
                    repository.Edit(order);
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
            Order order = repository.Get(id);

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
                    repository.Edit(order);

                    RestClient clientTotalEmail = new RestClient("https://localhost:5021/emails/total/");
                    var requestTotalEmail = new RestRequest();
                    var responseTotalEmail = clientTotalEmail.GetAsync<int>(requestTotalEmail);
                    responseTotalEmail.Wait();
                    int totalEmails = responseTotalEmail.Result;

                    RestClient clientCustomer = new RestClient("https://localhost:5011/customers/");
                    var requestCustomer = new RestRequest(order.CustomerId.ToString());
                    var responseCustomer = clientCustomer.GetAsync<Customer>(requestCustomer);
                    responseCustomer.Wait();
                    Customer customer = responseCustomer.Result;

                    Email email = new Email();
                    email.Id = totalEmails + 1;
                    email.Destination = customer.Email;
                    email.Content = String.Format("Order with id: {0}, has been shipped.", order.Id.ToString());

                    RestClient clientSendEmail = new RestClient("https://localhost:5021/emails/");
                    var requestSendEmail = new RestRequest();
                    requestSendEmail.AddJsonBody(email);
                    var responseSendEmail = clientSendEmail.PostAsync(requestSendEmail);
                    responseSendEmail.Wait();

                    if (!responseSendEmail.IsCompletedSuccessfully)
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

    }
}

