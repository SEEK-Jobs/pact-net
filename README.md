# PactNet
[![Build status](https://ci.appveyor.com/api/projects/status/5h4t9oerlhqcnwm8/branch/master?svg=true)](https://ci.appveyor.com/project/SEEKJobs/pact-net/branch/master)  
A .NET implementation of the Ruby consumer driven contract library, [Pact](https://github.com/realestate-com-au/pact).  
Pact is based off the specification found at https://github.com/pact-foundation/pact-specification.  

PactNet primarily provides a fluent .NET DSL for describing HTTP requests that will be made to a service provider and the HTTP responses the consumer expects back to function correctly.  
In documenting the consumer interactions, we can replay them on the provider and ensure the provider responds as expected. This basically gives us complete test symmetry and removes the basic need for integrated tests.  
PactNet also has the ability to support other mock providers should we see fit.

PactNet is Version 2.0 compliant, and we now use the [Ruby standalone engine](https://github.com/pact-foundation/pact-ruby-standalone) as we move towards a common core approach. To enable Version 2.0 support,  make sure you supply a `PactConfig` object with `SpecificationVersion = "2.0.0"` when creating the `PactBuilder`.  

In reaching Version 2.0 compliance, we have made some breaking changes. This readme details the current latest version.  
See [Version 1.0 readme](https://github.com/pact-foundation/pact-net/blob/master/README_v1.md) for the previous version.  

From the [Pact Specification repo](https://github.com/pact-foundation/pact-specification)

> "Pact" is an implementation of "consumer driven contract" testing that allows mocking of responses in the consumer codebase, and verification of the interactions in the provider codebase. The initial implementation was written in Ruby for Rack apps, however a consumer and provider may be implemented in different programming languages, so the "mocking" and the "verifying" steps would be best supported by libraries in their respective project's native languages. Given that the pact file is written in JSON, it should be straightforward to implement a pact library in any language, however, to get the best experience and most reliability of out mixing pact libraries, the matching logic for the requests and responses needs to be identical. There is little confidence to be gained in having your pacts "pass" if the logic used to verify a "pass" is inconsistent between implementations.

Read more about Pact and the problems it solves at https://docs.pact.io/

Please feel free to contribute, we do accept pull requests. This solution has been built using VS2017, you will need it to open this project.

## History
PactNet was initially built at [SEEK](https://www.seek.com.au/) to help solve some of the challenges faced with testing across service boundaries.  
The project now lives in the pact-foundation GH organisation, to help group and support the official Pact libraries.  
Massive thanks to the SEEK team for all the time and hard work put into this library.

[![SEEK](https://raw.githubusercontent.com/pact-foundation/pact-net/master/seek.png "SEEK")](https://www.seek.com.au/)


## Known Issues
1. When debugging a test locally (either consumer or provider) if you click the stop button in your test runner, it will abort the process abruptly and the ruby runtime will not get cleaned up. If you do this, simply kill the ruby process from your task/process manager. We recommend you play the test through to the end to avoid this issue. See https://github.com/pact-foundation/pact-net/issues/108 for more details.

## Usage
Below are some samples of usage.  
For examples of Version 2 usage, please see the [Samples](https://github.com/pact-foundation/pact-net/blob/master/Samples/EventApi/Consumer.Tests/EventsApiConsumerTests.cs).  

We have also written some `//NOTE:` comments inline in the code to help explain what certain calls do.

**A few others things to note:**
1. When using `Match.Regex` you must supply a valid Ruby regular expression, as we currently use the Ruby core engine.

### Installing

Via Nuget  

**Windows**  
https://www.nuget.org/packages/PactNet.Windows  
`Install-Package PactNet.Windows`

**OSX**  
https://www.nuget.org/packages/PactNet.OSX  
`Install-Package PactNet.OSX`

**Linux x64 (64-bit)**  
https://www.nuget.org/packages/PactNet.Linux.x64  
`Install-Package PactNet.Linux.x64`

**Linux x86 (32-bit)**  
https://www.nuget.org/packages/PactNet.Linux.x86  
`Install-Package PactNet.Linux.x86`

### Service Consumer

#### 1. Build your client
Which may look something like this.

```c#
public class SomethingApiClient
{
  private readonly HttpClient _client;

  public SomethingApiClient(string baseUri = null)
  {
    _client = new HttpClient { BaseAddress = new Uri(baseUri ?? "http://my-api") };
  }

  public Something GetSomething(string id)
  {
    string reasonPhrase;

    var request = new HttpRequestMessage(HttpMethod.Get, "/somethings/" + id);
    request.Headers.Add("Accept", "application/json");

    var response = _client.SendAsync(request);

    var content = response.Result.Content.ReadAsStringAsync().Result;
    var status = response.Result.StatusCode;

    reasonPhrase = response.Result.ReasonPhrase; //NOTE: any Pact mock provider errors will be returned here and in the response body

    request.Dispose();
    response.Dispose();

    if (status == HttpStatusCode.OK)
    {
      return !String.IsNullOrEmpty(content) ?
        JsonConvert.DeserializeObject<Something>(content)
        : null;
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

  public int MockServerPort { get { return 9222; } }
  public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }

  public ConsumerMyApiPact()
  {
    PactBuilder = new PactBuilder(); //Defaults to specification version 1.1.0, uses default directories. PactDir: ..\..\pacts and LogDir: ..\..\logs
    //or
    PactBuilder = new PactBuilder(new PactConfig { SpecificationVersion = "2.0.0" }); //Configures the Specification Version
    //or
    PactBuilder = new PactBuilder(new PactConfig { PactDir = @"..\pacts", LogDir = @"c:\temp\logs" }); //Configures the PactDir and/or LogDir.

    PactBuilder
      .ServiceConsumer("Consumer")
      .HasPactWith("Something API");

    MockProviderService = PactBuilder.MockService(MockServerPort); //Configure the http mock server
    //or
    MockProviderService = PactBuilder.MockService(MockServerPort, true); //By passing true as the second param, you can enabled SSL. A self signed SSL cert will be provisioned by default.
	//or
	MockProviderService = PactBuilder.MockService(MockServerPort, true, sslCert: sslCert, sslKey: sslKey); //By passing true as the second param and an sslCert and sslKey, you can enabled SSL with a custom certificate. See "Using a Custom SSL Certificate" for more details.
    //or
    MockProviderService = PactBuilder.MockService(MockServerPort, new JsonSerializerSettings()); //You can also change the default Json serialization settings using this overload    
    //or
    MockProviderService = PactBuilder.MockService(MockServerPort, host: IPAddress.Any); //By passing host as IPAddress.Any, the mock provider service will bind and listen on all ip addresses
    
  }

  public void Dispose()
  {
    PactBuilder.Build(); //NOTE: Will save the pact file once finished
  }
}

```

#### 3. Write your test
Create a new test case and implement it.

```c#
public class SomethingApiConsumerTests : IClassFixture<ConsumerMyApiPact>
{
  private IMockProviderService _mockProviderService;
  private string _mockProviderServiceBaseUri;

  public SomethingApiConsumerTests(ConsumerMyApiPact data)
  {
    _mockProviderService = data.MockProviderService;
    _mockProviderService.ClearInteractions(); //NOTE: Clears any previously registered interactions before the test is run
    _mockProviderServiceBaseUri = data.MockProviderServiceBaseUri;
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
        Headers = new Dictionary<string, object>
        {
          { "Accept", "application/json" }
        }
      })
      .WillRespondWith(new ProviderServiceResponse
      {
        Status = 200,
        Headers = new Dictionary<string, object>
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

    _mockProviderService.VerifyInteractions(); //NOTE: Verifies that interactions registered on the mock provider are called at least once
  }
}
```

#### 4. Run the test 
If you now navigate to [RepositoryRoot]/pacts you will see the pact file your test generated. Take a moment to have a look at what it contains which is a JSON representation of the mocked requests your test made.

Everything should be green. 

Note: we advise using a TDD approach when using this library, however we will leave it up to you.  
Likely you will be creating a skeleton client, describing the pact, write the failing test, implement the skeleton client, run the test to make sure it passes, then rinse and repeat.

### Service Provider

#### 1. Create the API
You can create the API using whatever framework you like, however this example will use ASP.NET Web API 2 with Owin.

Note: We have removed to support for Microsoft.Owin.Testing, as we have moved to using a shared Pact core. You will now be required to start the API and listen on the correct port, as part of the test.

#### 2. Tell the provider it needs to honour the pact
Create a new test case within your service provider test project, using whatever test framework you like (in this case we used xUnit). PactUri method should refer to the pact json file, created when the Consumer Test was run.

```c#
public class SomethingApiTests
{
  [Fact]
  public void EnsureSomethingApiHonoursPactWithConsumer()
  {
    //Arrange
    const string serviceUri = "http://localhost:9222";
    var config = new PactVerifierConfig
    {
        Outputters = new List<IOutput> //NOTE: We default to using a ConsoleOutput, however xUnit 2 does not capture the console output, so a custom outputter is required.
        {
            new XUnitOutput(_output)
        },
		CustomHeader = new KeyValuePair<string, string>("Authorization", "Basic VGVzdA=="), //This allows the user to set a request header that will be sent with every request the verifier sends to the provider
		Verbose = true //Output verbose verification logs to the test output
    };

    using (WebApp.Start<TestStartup>(serviceUri))
    {
        //Act / Assert
        IPactVerifier pactVerifier = new PactVerifier(config);
        pactVerifier
            .ProviderState($"{serviceUri}/provider-states")
            .ServiceProvider("Something API", serviceUri)
            .HonoursPactWith("Consumer")
            .PactUri("..\\..\\..\\Consumer.Tests\\pacts\\consumer-something_api.json")
            //or
            .PactUri("http://pact-broker/pacts/provider/Something%20Api/consumer/Consumer/latest") //You can specify a http or https uri
            //or
            .PactUri("http://pact-broker/pacts/provider/Something%20Api/consumer/Consumer/latest", new PactUriOptions("someuser", "somepassword")) //You can also specify http/https basic auth details
            .Verify();
    }
  }
}
```

```c#
public class TestStartup
{
    public void Configuration(IAppBuilder app)
    {
        var apiStartup = new Startup(); //This is your standard OWIN startup object
        app.Use<ProviderStateMiddleware>();

        apiStartup.Configuration(app);
    }
}
```

```c#
public class ProviderStateMiddleware
{
    private const string ConsumerName = "Event API Consumer";
    private readonly Func<IDictionary<string, object>, Task> m_next;
    private readonly IDictionary<string, Action> _providerStates;

    public ProviderStateMiddleware(Func<IDictionary<string, object>, Task> next)
    {
        m_next = next;
        _providerStates = new Dictionary<string, Action>
        {
            {
                "There is a something with id 'tester'",
                AddTesterIfItDoesntExist
            }
        };
    }

    private void AddTesterIfItDoesntExist()
    {
      //Add code to go an inject or insert the tester data
    }

    public async Task Invoke(IDictionary<string, object> environment)
    {
        IOwinContext context = new OwinContext(environment);

        if (context.Request.Path.Value == "/provider-states")
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            if (context.Request.Method == HttpMethod.Post.ToString() &&
                context.Request.Body != null)
            {
                string jsonRequestBody;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    jsonRequestBody = reader.ReadToEnd();
                }

                var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

                //A null or empty provider state key must be handled
                if (providerState != null &&
                    !IsNullOrEmpty(providerState.State) &&
                    providerState.Consumer == ConsumerName)
                {
                    _providerStates[providerState.State].Invoke();
                }

                await context.Response.WriteAsync(Empty);
            }
        }
        else
        {
            await m_next.Invoke(environment);
        }
    }
}
```

```c#
public class ProviderState
{
    public string Consumer { get; set; }
    public string State { get; set; }
}
```

```c#
public class XUnitOutput : IOutput
{
    private readonly ITestOutputHelper _output;

    public XUnitOutput(ITestOutputHelper output)
    {
        _output = output;
    }

    public void WriteLine(string line)
    {
        _output.WriteLine(line);
    }
}
```

#### 3. Run the test
Everything should be green

Again, please note: we advise using a TDD approach when using this library, however we will leave it up to you.

For further examples please refer to the [Samples](https://github.com/pact-foundation/pact-net/tree/master/Samples) in the solution.

### Using a Custom SSL Certificate
When creating the MockProviderService you can use a custom SSL cert, which allows the use of a valid installed certificate without requiring any hacks to ignore certificate validation errors.

#### 1. Generate a custom SSL certificate
The simplest way to generate a private key and self-signed certificate for localhost is with this openssl command:

``` 
openssl req -x509 -out localhost.crt -keyout localhost.key \
  -newkey rsa:2048 -nodes -sha256 \
  -subj '/CN=localhost' -extensions EXT -days 365 -config <( \
   printf "[dn]\nCN=localhost\n[req]\ndistinguished_name = dn\n[EXT]\nsubjectAltName=DNS:localhost\nkeyUsage=digitalSignature\nextendedKeyUsage=serverAuth")
```

The above script will generate two files:

- localhost.crt
- localhost.key

Ref. [certificates-for-localhost](https://letsencrypt.org/docs/certificates-for-localhost/).

#### 2. Add Certificate to Certificate Store
For windows, import the localhost.crt to the 'Trust Root Certification Authorities' of the Current User Certificates.

### 3. Use the Certificate
```
public string MockProviderServiceBaseUri => $"https://localhost:{MockServerPort}";
var sslCrt = @"{PathTo}\localhost.crt";
var sslKey = @"{PathTo}\localhost.key";
MockProviderService = PactBuilder.MockService(MockServerPort, true, IPAddress.Any, sslCrt, sslKey);
```

### Message queues support:

configuring a pact between a publisher and a subscriber is now possible:
For further examples please refer to the [Samples](https://github.com/pact-foundation/pact-net/tree/master/Samples) in the solution.

### Publishing Pacts to a Broker

The Pact broker is a useful tool that can be used to share pacts between the consumer and provider. In order to make this easy, below are a couple of options for publishing your Pacts to a Pact Broker.

#### Using PowerShell on your build server
[Checkout this gist](https://gist.github.com/neilcampbell/bc1fb7d409425894ece0) to see an example of how you can do this.

#### Using the C# client
If you use build tools like Fake and Cake, you may want create a broker publish task and using the PactPublisher.

```c#
var pactPublisher = new PactPublisher("http://test.pact.dius.com.au", new PactUriOptions("username", "password"));
pactPublisher.PublishToBroker(
    "..\\..\\..\\Samples\\EventApi\\Consumer.Tests\\pacts\\event_api_consumer-event_api.json",
    "1.0.2", new [] { "master" });
```

### Publishing Provider Verification Results to a Broker
This feature allows the result of the Provider verification to be pushed to the broker and displayed on the index page.
In order for this to work you must set the ProviderVersion, PublishVerificationResults and use a pact broker uri. If you do not use a broker uri no verification results will be published. See the code snippet code below.
For more info and compatibility details [refer to this](https://github.com/pact-foundation/pact_broker/wiki/Provider-verification-results).

```c#
var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");

//Assuming build number is only set in the CI environment
var config = new PactVerifierConfig
{
	ProviderVersion = !string.IsNullOrEmpty(buildNumber) ? buildNumber : null, //NOTE: This is required for this feature to work
	PublishVerificationResults = !string.IsNullOrEmpty(buildNumber)
};
IPactVerifier pactVerifier = new PactVerifier(config);
pactVerifier
	.ServiceProvider("Something Api", serviceUri)
	.HonoursPactWith("Consumer")
	.PactUri("http://pact-broker/pacts/provider/Something%20Api/consumer/Consumer/latest") //NOTE: This must be a pact broker url for this feature to work
	.Verify();
```

#### Further Documentation

* [PactNet Workshop using .NET Core](https://github.com/tdshipley/pact-workshop-dotnet-core-v1)
