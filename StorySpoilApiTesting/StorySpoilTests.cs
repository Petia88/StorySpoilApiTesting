using RestSharp;
using RestSharp.Authenticators;
using StorySpoilApiTesting.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoilApiTesting
{
    public class StorySpoilTests
    {
        private RestClient client;
        private const string BASE_URL = "https://d5wfqm7y6yb3q.cloudfront.net";
        private const string USERNAME = "petia";
        private const string PASSWORD = "123456";

        private static string storyId;
        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(USERNAME, PASSWORD);
            var options = new RestClientOptions(BASE_URL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            RestClient authClient = new RestClient(BASE_URL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new
            {
                username,
                password
            });
            var response = authClient.Execute(request, Method.Post);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Assess Token is null or white space.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected responce type {response.StatusCode}, with data {response.Content}");
            }
        }

        [Test, Order(1)]
        public void CreateNewStory_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var newStory = new StoryDTO
            {
                Title = "New Test Title",
                Description = "Some Description",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);

            //Act
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
            Assert.That(responseData.StoryId, Is.Not.Null);

            storyId = responseData.StoryId;
        }

        [Test, Order(2)]
        public void EditStory_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var requestData = new StoryDTO()
            {
                Title = "Edited Test Title",
                Description = "Test Description with edited title"
            };
            var request = new RestRequest($"/api/Story/Edit/{storyId}");
            request.AddJsonBody(requestData);

            //Act
            var response = client.Execute(request, Method.Put);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void DeleteStory_WithCorrectData_ShouldSucceed()
        {
            //Arrange
            var request = new RestRequest($"/api/Story/Delete/{storyId}");
            
            //Act
            var response = client.Execute(request, Method.Delete);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(4)]
        public void CreateNewStory_WithIncorrectData_ShouldFail()
        {
            //Arrange
            var newStory = new StoryDTO
            { 
                Description = "Some Description",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newStory);

            //Act
            var response = this.client.Execute(request);
           
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
        }

        [Test, Order(5)]
        public void EditStory_WithIncorrectData_ShouldFail()
        {
            //Arrange
            var requestData = new StoryDTO()
            {
                Title = "Edited Test Title",
                Description = "Test Description with edited title"
            };
            var request = new RestRequest($"/api/Story/Edit/112222333355");
            request.AddJsonBody(requestData);

            //Act
            var response = client.Execute(request, Method.Put);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(responseData.Msg, Is.EqualTo("No spoilers..."));
        }

        [Test, Order(6)]
        public void DeleteStory_WithIncorrectData_ShouldFail()
        {
            //Arrange
            var request = new RestRequest($"/api/Story/Delete/XXXXXXXXX");

            //Act
            var response = client.Execute(request, Method.Delete);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseData.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }
    }
}