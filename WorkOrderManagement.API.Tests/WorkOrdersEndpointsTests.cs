using System.Net.Http.Json;
using System.Net;
using WorkOrderManagement.API.Domain;
using WorkOrderManagement.API.Infrastructure;


namespace WorkOrderManagement.API.Tests
{
    public class WorkOrdersEndpointsTests : IClassFixture<CustomWebApplicationFactory> //, IAsyncLifetime
    {

        private readonly HttpClient _client;
        //private readonly CustomWebApplicationFactory _factory;

        public WorkOrdersEndpointsTests(CustomWebApplicationFactory factory)
        {
            //_factory = factory;
            _client = factory.CreateClient();
        }


        //Task IAsyncLifetime.InitializeAsync()
        //{
        //    _factory.ResetAndSeed();
        //    return Task.CompletedTask;
        //}

        //public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task Get_All_WorkOrders_Returns_All_WO()
        {
            // Act
            var response = await _client.GetAsync("/api/workorders");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var items = await response.Content.ReadFromJsonAsync<List<WorkOrder>>();
            Assert.NotNull(items);
            Assert.Equal(3, items.Count);
        }

        [Fact]
        public async Task Search_By_Technician_And_Client_Returns_Filtered_WorkOrders()
        {


            var response = await _client.GetAsync("/api/workorders/search?technicianId=1&clientId=1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var items = await response.Content.ReadFromJsonAsync<List<WorkOrder>>();
            Assert.NotNull(items);
            Assert.Single(items);
            Assert.NotNull(items[0].Information);
            Assert.Equal(("WO 1"),items[0].Information);
        }
    }



}