
        using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OtpGenerationApi.services
{
    public class TwilioOtpService
    {
    private readonly string twilioAccountSid;
    private readonly string twilioAuthToken;
    private readonly string twilioPhoneNumber;

    public TwilioOtpService(string accountSid, string authToken, string phoneNumber)
    {
        twilioAccountSid = accountSid;
        twilioAuthToken = authToken;
        twilioPhoneNumber = phoneNumber;

        TwilioClient.Init(accountSid, authToken);
    }

    public async Task<string> GenerateOtp()
    {
        // Generate a 6-digit random OTP
        Random random = new Random();
        int otp = random.Next(100000, 999999);

        return otp.ToString();
    }

    public async Task<bool> SendOtpViaTwilio(string toPhoneNumber, string otp)
    {
        try
        {
            var message = MessageResource.Create(
                to: new PhoneNumber(toPhoneNumber),
                from: new PhoneNumber(twilioPhoneNumber),
                body: $"Your OTP for password reset: {otp}");

            // You can log the message SID if needed
            Console.WriteLine($"OTP Sent. Message SID: {message.Sid}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending OTP: {ex.Message}");
            return false;
        }
    }
}

    }
