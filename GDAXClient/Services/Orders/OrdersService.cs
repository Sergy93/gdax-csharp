﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GDAXClient.Authentication;
using GDAXClient.HttpClient;
using GDAXClient.Services.HttpRequest;
using GDAXClient.Services.Orders.Models;
using GDAXClient.Services.Orders.Models.Responses;
using GDAXClient.Shared;
using GDAXClient.Utilities;
using GDAXClient.Utilities.Extensions;
using Newtonsoft.Json;

namespace GDAXClient.Services.Orders
{
    public class OrdersService : AbstractService
    {
        private readonly IHttpClient httpClient;

        private readonly IAuthenticator authenticator;

        private readonly IQueryBuilder queryBuilder;

        public OrdersService(
            IHttpClient httpClient,
            IHttpRequestMessageService httpRequestMessageService,
            IAuthenticator authenticator,
            IQueryBuilder queryBuilder)
                : base(httpClient, httpRequestMessageService)

        {
            this.httpClient = httpClient;
            this.authenticator = authenticator;
            this.queryBuilder = queryBuilder;
        }

        public async Task<OrderResponse> PlaceMarketOrderAsync(
            OrderSide side, 
            ProductType productPair, 
            decimal size)
        {
            var newOrder = JsonConvert.SerializeObject(new Order
            {
                side = side.ToString().ToLower(),
                product_id = productPair.ToDasherizedUpper(),
                type = OrderType.Market.ToString().ToLower(),
                size = size
            });

            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Post, authenticator, "/orders", newOrder);
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);
            var orderResponse = JsonConvert.DeserializeObject<OrderResponse>(contentBody);

            return orderResponse;
        }

        public async Task<OrderResponse> PlaceLimitOrderAsync(
            OrderSide side, 
            ProductType productPair, 
            decimal size, 
            decimal price, 
            TimeInForce timeInForce = TimeInForce.Gtc, 
            bool postOnly = true)
        {
            var newOrder = JsonConvert.SerializeObject(new Order
            {
                side = side.ToString().ToLower(),
                product_id = productPair.ToDasherizedUpper(),
                type = OrderType.Limit.ToString().ToLower(),
                price = price,
                size = size
            });

            var queryString = queryBuilder.BuildQuery(
                new KeyValuePair<string, string>("time_in_force", timeInForce.ToString().ToUpperInvariant()),
                new KeyValuePair<string, string>("post_only", postOnly.ToString().ToLower()));
            
            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Post, authenticator, "/orders"  + queryString, newOrder).ConfigureAwait(false);
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);
            var orderResponse = JsonConvert.DeserializeObject<OrderResponse>(contentBody);

            return orderResponse;
        }

        public async Task<OrderResponse> PlaceLimitOrderAsync(
            OrderSide side, 
            ProductType productPair, 
            decimal size, 
            decimal price, 
            DateTime cancelAfter, 
            bool postOnly = true)
        {
            var newOrder = JsonConvert.SerializeObject(new Order
            {
                side = side.ToString().ToLower(),
                product_id = productPair.ToDasherizedUpper(),
                type = OrderType.Limit.ToString().ToLower(),
                price = price,
                size = size
            });

            var queryString = queryBuilder.BuildQuery(
                new KeyValuePair<string, string>("time_in_force", "GTT"),
                new KeyValuePair<string, string>("cancel_after", cancelAfter.Minute + "," + cancelAfter.Hour + "," + cancelAfter.Day),
                new KeyValuePair<string, string>("post_only", postOnly.ToString().ToLower()));

            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Post, authenticator, "/orders" + queryString, newOrder);
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);            
            var orderResponse = JsonConvert.DeserializeObject<OrderResponse>(contentBody);

            return orderResponse;
        }
       
        public async Task<CancelOrderResponse> CancelAllOrdersAsync()
        {
            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Delete, authenticator, "/orders");
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);
            var orderResponse = JsonConvert.DeserializeObject<IEnumerable<Guid>>(contentBody);

            return new CancelOrderResponse
            {
                OrderIds = orderResponse
            };
        }

        public async Task<CancelOrderResponse> CancelOrderByIdAsync(string id)
        {
            var httpRequestResponse = await SendHttpRequestMessageAsync(HttpMethod.Delete, authenticator, $"/orders/{id}");

            if (httpRequestResponse == null)
            {
                return new CancelOrderResponse
                {
                    OrderIds = Enumerable.Empty<Guid>()
                };
            }

            return new CancelOrderResponse
            {
                OrderIds = new List<Guid> { new Guid(id) }
            };
        }

        public async Task<IList<IList<OrderResponse>>> GetAllOrdersAsync(int limit = 100)
        {
            var httpResponseMessage = await SendHttpRequestMessagePagedAsync<OrderResponse>(HttpMethod.Get, authenticator, $"/orders?limit={limit}");

            return httpResponseMessage;
        }

        public async Task<OrderResponse> GetOrderByIdAsync(string id)
        {
            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Get, authenticator, $"/orders/{id}");
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);
            var orderResponse = JsonConvert.DeserializeObject<OrderResponse>(contentBody);

            return orderResponse;
        }
    }
}
