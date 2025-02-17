using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.AspNetCore.AccessTokenValidation;
using Tests.Infrastructure;
using Xunit;

namespace Tests
{
    public class ForwarderTests
    {
        [Fact]
        public async Task no_forwarder_should_200()
        {
            var server = new Server();
            
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task introspection_forwarder_with_JWT_should_200()
        {
            var server = new Server
            {
                AddForwarder = true
            };
        
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "header.payload.signature");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task introspection_forwarder_with_no_token_should_200()
        {
            var server = new Server
            {
                AddForwarder = true
            };
        
            var client = server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        [Fact]
        public void introspection_forwarder_with_default_scheme_and_reference_token_should_call_Introspection_handler()
        {
            var server = new Server
            {
                AddForwarder = true
            };
        
            var client = server.CreateClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "reference");
        
            Func<Task> act = async () => { await client.SendAsync(request); };
            act.Should().ThrowAsync<InvalidOperationException>()
                .Where(e => e.Message.StartsWith("No authentication handler is registered for the scheme 'Introspection'"));
        }
        
        [Fact]
        public void introspection_forwarder_with_foo_scheme_and_reference_token_should_call_foo_handler()
        {
            var server = new Server
            {
                AddForwarder = true,
                ForwardScheme = "foo"
            };
        
            var client = server.CreateClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "reference");
        
            Func<Task> act = async () => { await client.SendAsync(request); };
            act.Should().ThrowAsync<InvalidOperationException>()
                .Where(e => e.Message.StartsWith("No authentication handler is registered for the scheme 'foo'"));
        }
        
        // [Fact]
        // public async Task test_selector_no_handler_unknown_token_should_401()
        // {
        //     var server = new Server
        //     {
        //         AccessTokenOptions = o =>
        //         {
        //             o.SchemeSelector = TestSelector.Func;
        //         }
        //     };
        //     
        //     var client = server.CreateClient();
        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
        //     request.Headers.Authorization = new AuthenticationHeaderValue("unknown", "token");
        //
        //     var response = await client.SendAsync(request);
        //
        //     response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        // }
        //
        // [Fact]
        // public async Task test_selector_no_handler_known_token_should_throw()
        // {
        //     var server = new Server
        //     {
        //         AccessTokenOptions = o =>
        //         {
        //             o.DefaultScheme = "test";
        //             o.SchemeSelector = TestSelector.Func;
        //         }
        //     };
        //     
        //     var client = server.CreateClient();
        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
        //     request.Headers.Authorization = new AuthenticationHeaderValue("known", "token");
        //
        //
        //     Func<Task> act = async () => { await client.SendAsync(request); };
        //     await act.Should().ThrowAsync<InvalidOperationException>();
        // }
        //
        // [Fact]
        // public async Task test_selector_test_handler_no_token_should_401()
        // {
        //     var server = new Server
        //     {
        //         AddTestHandler = true,
        //         AccessTokenOptions = o =>
        //         {
        //             o.SchemeSelector = TestSelector.Func;
        //         }
        //     };
        //     
        //     var client = server.CreateClient();
        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
        //
        //     var response = await client.SendAsync(request);
        //
        //     response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        // }
        //
        // [Fact]
        // public async Task test_selector_test_handler_known_token_should_200()
        // {
        //     var server = new Server
        //     {
        //         AddTestHandler = true,
        //         AccessTokenOptions = o =>
        //         {
        //             o.DefaultScheme = "test";
        //             o.SchemeSelector = TestSelector.Func;
        //         }
        //     };
        //     
        //     var client = server.CreateClient();
        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
        //     request.Headers.Authorization = new AuthenticationHeaderValue("known", "token");
        //
        //     var response = await client.SendAsync(request);
        //
        //     response.StatusCode.Should().Be(HttpStatusCode.OK);
        // }
        //
        // [Fact]
        // public async Task handlers_and_token_should_200()
        // {
        //     var server = new Server
        //     {
        //         AddTestHandler = true, 
        //         AccessTokenOptions = o => { o.SchemeSelector = context => "test"; }
        //     };
        //     
        //     var client = server.CreateClient();
        //     
        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
        //     request.Headers.Authorization = new AuthenticationHeaderValue("prefix", "token");
        //
        //     var response = await client.SendAsync(request);
        //
        //     response.StatusCode.Should().Be(HttpStatusCode.OK);
        // }
        //
        // [Fact]
        // public async Task Introspection_setup_with_no_token_should_call_bearer_handler()
        // {
        //     var server = new Server
        //     {
        //         AccessTokenOptions = o =>
        //         {
        //             o.DefaultScheme = "Bearer";
        //             o.SchemeSelector = JwtAndIntrospectionSelector.Func;
        //         }
        //     };
        //
        //     var client = server.CreateClient();
        //     
        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
        //
        //     Func<Task> act = async () => { await client.SendAsync(request); };
        //     act.Should().Throw<InvalidOperationException>()
        //         .Where(e => e.Message.StartsWith("No authentication handler is registered for the scheme 'Bearer'"));
        // }
        //
        // [Fact]
        // public async Task Introspection_setup_with_JWT_should_call_bearer_handler()
        // {
        //     var server = new Server
        //     {
        //         AccessTokenOptions = o =>
        //         {
        //             o.DefaultScheme = "Bearer";
        //             o.SchemeSelector = JwtAndIntrospectionSelector.Func;
        //         }
        //     };
        //
        //     var client = server.CreateClient();
        //     
        //     var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
        //     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "header.payload.signature");
        //
        //     Func<Task> act = async () => { await client.SendAsync(request); };
        //     act.Should().Throw<InvalidOperationException>()
        //         .Where(e => e.Message.StartsWith("No authentication handler is registered for the scheme 'Bearer'"));
        // }
        //
        
    }
}