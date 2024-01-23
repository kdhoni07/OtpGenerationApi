using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Ocsp;
using Twilio;
using Twilio.Rest.Verify.V2.Service;
using Twilio.Types;
using System.IdentityModel.Tokens.Jwt;


namespace OtpGenerationApi.services
{
    public class OtpGeneration
    {

        dbServices ds = new dbServices();
        private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _service_config = new Dictionary<string, string>();


        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        public OtpGeneration()
        {

            jwt_config["Key"] = appsettings["jwt_config:Key"].ToString();
            jwt_config["Issuer"] = appsettings["jwt_config:Issuer"].ToString();
            jwt_config["Audience"] = appsettings["jwt_config:Audience"].ToString();
            jwt_config["Subject"] = appsettings["jwt_config:Subject"].ToString();
            jwt_config["ExpiryDuration_app"] = appsettings["jwt_config:ExpiryDuration_app"].ToString();
            jwt_config["ExpiryDuration_web"] = appsettings["jwt_config:ExpiryDuration_web"].ToString();
        }

        public async Task<responseData> PasswordForget(requestData req)
        {
            responseData resData = new responseData();
            // resData.rData["rCode"] = 0;

            try
            {
                var mobile = req.addInfo["U_Mobile"].ToString();
                MySqlParameter[] para = new MySqlParameter[] {
                new MySqlParameter("@U_Mobile",mobile),
                new MySqlParameter("@New_Password",req.addInfo["New_Password"].ToString())

                };
                var sql = $"select * from Food_Table where U_Mobile=@U_Mobile;";
                var check = ds.ExecuteSQLName(sql, para);

                if (check != null && check[0].Count() > 0)
                {
                    var id = check[0][0]["ID"];

                    var claims = new[]
               {
                     new Claim("uid",id.ToString()),
                     new Claim("guid", cf.CalculateSHA256Hash(req.addInfo["guid"].ToString())),
                };
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_config["Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                    var tokenDescriptor = new JwtSecurityToken(issuer: jwt_config["Issuer"], audience: jwt_config["Audience"], claims: claims,
                        expires: DateTime.Now.AddMinutes(Int32.Parse(jwt_config["ExpiryDuration_app"])), signingCredentials: credentials);
                    var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

                    const string accountSid = "AC8ed02efc9177e43e469527909c57b6eb";
                    const string authToken = "afa0ef515f23e603ba230b8b781d67ba";
                    const string verifySid = "VA17debcf4203e2495268acead2239e843";

                    TwilioClient.Init(accountSid, authToken);

                    // Sending OTP

                    var verification = VerificationResource.Create(
                        to: new PhoneNumber("+91" + mobile).ToString(),
                        channel: "sms",
                        pathServiceSid: verifySid
                    );

                    Console.WriteLine($"Verification SID: {verification.Sid}");
                    //         // Verifying OTP
                    Console.Write("Please enter the OTP: ");
                    var otpCode = Console.ReadLine();

                    var verificationCheck = VerificationCheckResource.Create(
                        to: new PhoneNumber("+91" + mobile).ToString(),
                        code: otpCode,
                        pathServiceSid: verifySid
                    );
                    Console.WriteLine($"Verification Check Status: {verificationCheck.Status}");
                    resData.eventID = req.eventID;
                    resData.rData["rCode"] = 0;
                    resData.rData["rMessage"] = " New Password created Successfully";
                    resData.eventID = req.eventID;
                    resData.rData["rCode"] = 0;
                    resData.rData["rMessage"] = "Login Successfully";
                    resData.rData["ID"] = check[0][0]["ID"];
                    resData.rData["U_Mobile"] = check[0][0]["U_Mobile"];
                    resData.rData["U_Email"] = check[0][0]["U_Email"];
                    resData.rData["Token"] = token;
                    resData.rData["otp"] = otpCode;

                    var query1 = $"update Food_Table set U_Password=@New_Password where U_Mobile=@U_Mobile";
                    var update = ds.executeSQL(query1, para);

                    if (update != null)
                    {
                        resData.rData["rCode"] = 0;
                        resData.rData["rMessage"] = "Password created successfully";
                    }
                    else
                    {
                        resData.rData["rCode"] = 1;
                        resData.rData["rMessage"] = "Enter valid new password ";
                    }
                }
                else
                {

                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = " Oops... Error in Mobile Number ";

                }

            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = ex.Message;
            }

            return resData;
        }
    }
}