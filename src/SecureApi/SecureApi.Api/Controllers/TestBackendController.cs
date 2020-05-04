﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SecureApi.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestBackendController : ControllerBase
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<TestBackendController> logger;

        public TestBackendController(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<TestBackendController> logger)
        {
            this.clientFactory = clientFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        public class ApiCallDetails
        {
            public string AccessToken { get; set; }
            public string Body { get; set; }
            public int StatusCode { get; set; }
            public string Reason { get; set; }
        }

        [HttpGet]
        public async Task<ApiCallDetails> Get()
        {
            this.logger.LogInformation($"Executing {nameof(Get)}.");

            var response = await InvokeSpeakerService();

            this.logger.LogInformation($"Executed {nameof(Get)}.");
            
            return response;
        }

        private async Task<ApiCallDetails> InvokeSpeakerService()
        {
            string speakerApiUri = this.configuration["SpeakerApiUri"];

            var accessToken = await GenerateAccessToken();
            
            var httpClient = this.clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync(speakerApiUri);
            var body = await response.Content.ReadAsStringAsync();

            var callDetails = new ApiCallDetails
            {
                AccessToken = accessToken,
                Body = body,
                StatusCode = (int)response.StatusCode,
                Reason = response.ReasonPhrase
            };

            return callDetails;
        }

        private async Task<string> GenerateAccessToken()
        {
            string applicationIdUri = this.configuration["ApplicationIdUri"];
            var tenantId = this.configuration["ActiveDirectory:TenantId"];

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider
                                        .GetAccessTokenAsync(
                                                    applicationIdUri, tenantId: 
                                                    tenantId);
            return accessToken;
        }
    }
}