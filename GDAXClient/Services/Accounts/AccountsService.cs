﻿
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GDAXClient.Authentication;
using GDAXClient.HttpClient;
using GDAXClient.Services.Accounts.Models;
using GDAXClient.Services.HttpRequest;
using Newtonsoft.Json;

namespace GDAXClient.Services.Accounts
{
    public class AccountsService : AbstractService
    {
        private readonly IHttpClient httpClient;

        private readonly IAuthenticator authenticator;

        public AccountsService(
            IHttpClient httpClient,
            IHttpRequestMessageService httpRequestMessageService,
            IAuthenticator authenticator)
                : base(httpClient, httpRequestMessageService)
        {
            this.httpClient = httpClient;
            this.authenticator = authenticator;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsAsync()
        {
            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Get, authenticator, "/accounts");
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);
            var accountList = JsonConvert.DeserializeObject<List<Account>>(contentBody);

            return accountList;
        }

        public async Task<Account> GetAccountByIdAsync(Guid id)
        {
            var httpResponseMessage = await SendHttpRequestMessageAsync(HttpMethod.Get, authenticator, $"/accounts/{id}");
            var contentBody = await httpClient.ReadAsStringAsync(httpResponseMessage).ConfigureAwait(false);
            var account = JsonConvert.DeserializeObject<Account>(contentBody);

            return account;
        }

        public async Task<IList<IList<AccountHistory>>> GetAccountHistoryAsync(Guid id, int limit = 100)
        {
            var httpResponseMessage = await SendHttpRequestMessagePagedAsync<AccountHistory>(HttpMethod.Get, authenticator, $"/accounts/{id}/ledger?limit={limit}");

            return httpResponseMessage;
        }

        public async Task<IList<IList<AccountHold>>> GetAccountHoldsAsync(Guid id, int limit = 100)
        {
            var httpResponseMessage = await SendHttpRequestMessagePagedAsync<AccountHold>(HttpMethod.Get, authenticator, $"/accounts/{id}/holds?limit={limit}");

            return httpResponseMessage;
        }
    }
}
