using System;
using Twilio;
using Twilio.Rest.Verify.V2;

namespace OtpGenerationApi.services
{

class Program
{
    static void Main(string[] args)
    {
        // Find your Account SID and Auth Token at twilio.com/console
        // and set the environment variables. See http://twil.io/secure
        string accountSid = Environment.GetEnvironmentVariable("AC8ed02efc9177e43e469527909c57b6eb");
        string authToken = Environment.GetEnvironmentVariable("afa0ef515f23e603ba230b8b781d67ba");

        TwilioClient.Init(accountSid, authToken);

        var service = ServiceResource.Create(friendlyName: "My Verify Service");

        Console.WriteLine(service.Sid);
    }
}

    }
