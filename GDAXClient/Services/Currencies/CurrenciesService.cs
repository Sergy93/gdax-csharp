﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GDAXClient.Authentication;
using GDAXClient.HttpClient;
using GDAXClient.Services.HttpRequest;
using Newtonsoft.Json;

namespace GDAXClient.Services.Currencies
{
    public class CurrenciesService : AbstractService
    {
        private readonly IHttpClient httpClient;

        private readonly IAuthenticator authenticator;

        public CurrenciesService(
            IHttpClient httpClient,
            IHttpRequestMessageService httpRequestMessageService,
            IAuthenticator authenticator)
                : base(httpClient, httpRequestMessageService)
        {
            this.httpClient = httpClient;
            this.authenticator = authenticator;
        }

        public async Task<IEnumerable<Models.Currency>> GetAllCurrenciesAsync()
        {
            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Get, authenticator, "/currencies");
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);
            var currencies = JsonConvert.DeserializeObject<List<Models.Currency>>(contentBody);

            return currencies;
        }
    }
}
