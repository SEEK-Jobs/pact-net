# PactNet
[![Build status](https://ci.appveyor.com/api/projects/status/5h4t9oerlhqcnwm8/branch/master?svg=true)](https://ci.appveyor.com/project/SEEKJobs/pact-net/branch/master)  
A .NET implementation of the Ruby consumer driven contract library, [Pact](https://github.com/realestate-com-au/pact).  
Pact is based off the specification found at https://github.com/bethesque/pact_specification.  

PactNet primarily provides a fluent .NET DSL for describing HTTP requests that will be made to a service provider and the HTTP responses the consumer expects back to function correctly.  
In documenting the consumer interactions, we can replay them on the provider and ensure the provider responds as expected. This basically gives us complete test symmetry and removes the basic need for integrated tests.  
PactNet also has the ability to support other mock providers should we see fit.

PactNet is aiming to be Pact Specification Version 1.1 compliant (Version 2 is a WIP).  

From the [Pact Specification repo] (https://github.com/bethesque/pact_specification)

> "Pact" is an implementation of "consumer driven contract" testing that allows mocking of responses in the consumer codebase, and verification of the interactions in the provider codebase. The initial implementation was written in Ruby for Rack apps, however a consumer and provider may be implemented in different programming languages, so the "mocking" and the "verifying" steps would be best supported by libraries in their respective project's native languages. Given that the pact file is written in JSON, it should be straightforward to implement a pact library in any language, however, to get the best experience and most reliability of out mixing pact libraries, the matching logic for the requests and responses needs to be identical. There is little confidence to be gained in having your pacts "pass" if the logic used to verify a "pass" is inconsistent between implementations.

Read more about Pact and the problems it solves at https://github.com/realestate-com-au/pact

Please feel free to contribute, we do accept pull requests.

## Usage
Below are some samples of usage.  
We have also written some `//NOTE:` comments inline in the code to help explain what certain calls do.

### Installing

Via Nuget with [Install-Package PactNet](http://www.nuget.org/packages/PactNet)

### Service Consumer

#### 1. Build your client
Which may look something like this.

```c#
public class SomethingApiClient
{
	public string BaseUri { get; set; }

	public SomethingApiClient(string baseUri = null)
	{
		BaseUri = baseUri ?? "http://my-api";
	}
	
	public async Task<Something> GetSomething(string id)
	{
		string reasonPhrase;
		
		using (var client = new HttpClient { BaseAddress = new Uri(BaseUri) })
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "/somethings/" + id);
			request.Headers.Add("Accept", "application/json");

			var response = await client.SendAsync(request);

			var content = await response.Content.ReadAsStringAsync();
			var status = response.StatusCode;

			reasonPhrase = response.ReasonPhrase; //NOTE: any Pact mock provider errors will be returned here and in the response body
			
			request.Dispose();
			response.Dispose();
			
			if (status == HttpStatusCode.OK)
			{
				return !String.IsNullOrEmpty(content) ? 
					JsonConvert.DeserializeObject<Something>(content)
					: null;
			}
		}

		throw new Exception(reasonPhrase); 
	}
}
```

#### 2. Describe and configure the pact as a service consumer with a mock service
Create a new test case within your service consumer test project, using whatever test framework you like (in this case we used xUnit).  
This should only be instantiated once for the consumer you are testing.

```c#
public class ConsumerMyApiPact : IDisposable
{
	public IPactBuilder PactBuilder { get; private set; }
	public IMockProviderService MockProviderService { get; private set; }

	public int MockServerPort { get { return 1234; } }
	public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }

	public ConsumerMyApiPact()
	{
		PactBuilder = new PactBuilder(); //Uses default directories. PactDir: ..\..\pacts and LogDir: ..\..\logs
		//or
		PactBuilder = new PactBuilder(new PactConfig { PactDir = @"..\pacts", LogDir = @"c:\temp\logs" }); //Configures the PactDir and/or LogDir.
	
		PactBuilder
			.ServiceConsumer("Consumer")
			.HasPactWith("Something API");

		MockProviderService = PactBuilder.MockService(MockServerPort); //Configure the http mock server
		//or
		MockProviderService = PactBuilder.MockService(MockServerPort, true); //By passing true as the second param, you can enabled SSL. This will however require a valid SSL certificate installed and bound with netsh (netsh http add sslcert ipport=0.0.0.0:port certhash=thumbprint appid={app-guid}) on the machine running the test. See https://groups.google.com/forum/#!topic/nancy-web-framework/t75dKyfgzpg
		//or
		MockProviderService = PactBuilder.MockService(MockServerPort, bindOnAllAdapters: true); //By passing true as the bindOnAllAdapters parameter the http mock server will be able to accept external network requests, but will require admin privileges in order to run
		//or
		MockProviderService = PactBuilder.MockService(MockServerPort, new JsonSerializerSettings()); //You can also change the default Json serialization settings using this overload		
	}

	public void Dispose()
	{
		PactBuilder.Build(); //NOTE: Will save the pact file once finished
	}
}

public class SomethingApiConsumerTests : IUseFixture<ConsumerMyApiPact>
{
	private IMockProviderService _mockProviderService;
	private string _mockProviderServiceBaseUri;
		
	public void SetFixture(ConsumerMyApiPact data)
	{
		_mockProviderService = data.MockProviderService;
        _mockProviderServiceBaseUri = data.MockProviderServiceBaseUri;
		data.MockProviderService.ClearInteractions(); //NOTE: Clears any previously registered interactions before the test is run
	}
}
```

#### 3. Write your test
Create a new test case and implement it.


```c#
public class SomethingApiConsumerTests : IUseFixture<ConsumerMyApiPact>
{
	private IMockProviderService _mockProviderService;
	private string _mockProviderServiceBaseUri;
		
	public void SetFixture(ConsumerMyApiPact data)
	{
		_mockProviderService = data.MockProviderService;
        _mockProviderServiceBaseUri = data.MockProviderServiceBaseUri;
		data.MockProviderService.ClearInteractions(); //NOTE: Clears any previously registered interactions before the test is run
	}
	
	[Fact]
	public void GetSomething_WhenTheTesterSomethingExists_ReturnsTheSomething()
	{
		//Arrange
		_mockProviderService
			.Given("There is a something with id 'tester'")
			.UponReceiving("A GET request to retrieve the something")
			.With(new ProviderServiceRequest
			{
				Method = HttpVerb.Get,
				Path = "/somethings/tester",
				Headers = new Dictionary<string, string>
				{
					{ "Accept", "application/json" }
				}
			})
			.WillRespondWith(new ProviderServiceResponse
			{
				Status = 200,
				Headers = new Dictionary<string, string>
				{
					{ "Content-Type", "application/json; charset=utf-8" }
				},
				Body = new //NOTE: Note the case sensitivity here, the body will be serialised as per the casing defined
				{
					id = "tester",
					firstName = "Totally",
					lastName = "Awesome"
				}
			}); //NOTE: WillRespondWith call must come last as it will register the interaction
			
		var consumer = new SomethingApiClient(_mockProviderServiceBaseUri);
		
		//Act
		var result = consumer.GetSomething("tester");
		
		//Assert
		Assert.Equal("tester", result.id);
		
		_mockProviderService.VerifyInteractions(); //NOTE: Verifies that interactions registered on the mock provider are called once and only once
	}
}
```

#### 4. Run the test
Everything should be green

Note: we advise using a TDD approach when using this library, however we will leave it up to you.  
Likely you will be creating a skeleton client, describing the pact, write the failing test, implement the skeleton client, run the test to make sure it passes, then rinse and repeat.

### Service Provider

#### 1. Create the API
You can create the API using whatever framework you like, however we recommend using ASP.NET Web API 2 with Owin as it allows you to use the Microsoft.Owin.Testing Nuget package.  
Using the Microsoft.Owin.Testing package allows you to create an in memory version of your API to test against, which removes the pain of deployment and configuration when running the tests.

#### 2. Tell the provider it needs to honour the pact
Create a new test case within your service provider test project, using whatever test framework you like (in this case we used xUnit).

**a)** If using an Owin compatible API, like ASP.NET Web API 2 and using the Microsoft.Owin.Testing Nuget package **(preferred)**
```c#
public class SomethingApiTests
{
	[Fact]
	public void EnsureSomethingApiHonoursPactWithConsumer()
	{
		//Arrange
		IPactVerifier pactVerifier = new PactVerifier(() => {}, () => {}); //NOTE: You can supply setUp and tearDown, which will run before starting and after completing each individual verification.
		
		pactVerifier
			.ProviderState("There is a something with id 'tester'",
				setUp: AddTesterIfItDoesntExist); //NOTE: We also have tearDown

		//Act / Assert
		using (var testServer = TestServer.Create<Startup>()) //NOTE: This is using the Microsoft.Owin.Testing nuget package
		{
			pactVerifier
				.ServiceProvider("Something API", testServer.HttpClient)
				.HonoursPactWith("Consumer")
				.PactUri("../../../Consumer.Tests/pacts/consumer-something_api.json")
				//or
				.PactUri("http://pact-broker/pacts/provider/Something%20Api/consumer/Consumer/version/latest") //You can specify a http or https uri
				//or
				.PactUri("http://pact-broker/pacts/provider/Something%20Api/consumer/Consumer/version/latest", new PactUriOptions("someuser", "somepassword")) //You can also specify http/https basic auth details
				.Verify(); //NOTE: Optionally you can control what interactions are verified by specifying a providerDescription and/or providerState
		}
	}

	private void AddTesterIfItDoesntExist()
	{
		//Logic to add the 'tester' something
	}
}
```

or


**b)** If using a non Owin compatible API, you can provide your own HttpClient and point to the deployed API.
```c#
public class SomethingApiTests
{
	[Fact]
	public void EnsureSomethingApiHonoursPactWithConsumer()
	{
		//Arrange
		IPactVerifier pactVerifier = new PactVerifier(() => {}, () => {}); //NOTE: The supplied setUp and tearDown actions will run before starting and after completing each individual verification.
		
		pactVerifier
			.ProviderState("There is a something with id 'tester'",
				setUp: AddTesterIfItDoesntExist); //NOTE: We also have tearDown

		//Act / Assert
		using (var client = new HttpClient { BaseAddress = new Uri("http://api-address:9999") })
		{
			pactVerifier
				.ServiceProvider("Something API", client) //NOTE: You can also use your own client by using the ServiceProvider method which takes a Func<ProviderServiceRequest, ProviderServiceResponse>. You are then responsible for mapping and performing the actual provider verification HTTP request within that Func.
				.HonoursPactWith("Consumer")
				.PactUri("../../../Consumer.Tests/pacts/consumer-something_api.json")
				//or
				.PactUri("http://pact-broker/pacts/provider/Something%20Api/consumer/Consumer/version/latest") //You can specify a http or https uri
				//or
				.PactUri("http://pact-broker/pacts/provider/Something%20Api/consumer/Consumer/version/latest", new PactUriOptions("someuser", "somepassword")) //You can also specify http/https basic auth details
				.Verify(); //NOTE: Optionally you can control what interactions are verified by specifying a providerDescription and/or providerState
		}
	}

	private void AddTesterIfItDoesntExist()
	{
		//Logic to add the 'tester' something
	}
}
```

#### 4. Run the test
Everything should be green

Again, please note: we advise using a TDD approach when using this library, however we will leave it up to you.

For further examples please refer to the [Samples](https://github.com/SEEK-Jobs/pact-net/tree/master/Samples) in the solution.

#### Related Tools

You might also find the following tool and library helpful:

* [Pact based service simulator](https://github.com/SEEK-Jobs/SEEK.Automation.Phantom): leverage pacts to isolate your services
* [Pact based stubbing library](https://github.com/SEEK-Jobs/seek.automation.stub): leverage pacts during automation to stub dependencies 