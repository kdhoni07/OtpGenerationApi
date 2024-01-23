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

namespace OtpGenerationApi.services;


     public class Login
    {
        dbServices ds = new dbServices();
         private readonly Dictionary<string, string> jwt_config = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _service_config = new Dictionary<string, string>();

      
        IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
     public Login()
        {

            jwt_config["Key"] = appsettings["jwt_config:Key"].ToString();
            jwt_config["Issuer"] = appsettings["jwt_config:Issuer"].ToString();
            jwt_config["Audience"] = appsettings["jwt_config:Audience"].ToString();
            jwt_config["Subject"] = appsettings["jwt_config:Subject"].ToString();
            jwt_config["ExpiryDuration_app"] = appsettings["jwt_config:ExpiryDuration_app"].ToString();
            jwt_config["ExpiryDuration_web"] = appsettings["jwt_config:ExpiryDuration_web"].ToString();
        }

   public async Task<responseData> LoginData(requestData req)
        {
            responseData resData = new responseData();
            try
            {
               string userid = req.addInfo["UserId"].ToString();
                bool isemail = IsValidEmail(userid);
                bool ismobile = IsValidMobileNumber(userid);
                string email;
                if (isemail)
                {
                    email = "U_Email";
                }
                else if(ismobile)
                {
                    email = "U_Mobile";
                }
                else
                {
                    email = " ";
                }
                MySqlParameter[] param = new MySqlParameter[]
                {
                    new MySqlParameter("@UserId",userid),
                    new MySqlParameter("@U_Password",req.addInfo["U_Password"].ToString())
                };
                var query = $"select * from Food_Table where {email}=@UserId AND U_Password=@U_Password;";
                var dataquery = ds.ExecuteSQLName(query, param);
                if (dataquery!= null)
                {
                     var id = dataquery[0][0]["ID"];

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
                    resData.eventID = req.eventID;
                    resData.rData["rCode"] = 0;
                    resData.rData["rMessage"] = "Login Successfully";
                     resData.rData["ID"] = dataquery[0][0]["ID"];
                    resData.rData["U_Mobile"] = dataquery[0][0]["U_Mobile"];
                    resData.rData["U_Email"] = dataquery[0][0]["U_Email"];
                     resData.rData["Token"] = token;
                }
                else
                {
                    resData.rData["rCode"] = 1;
                    resData.rData["rMessage"] = "Invalid Details";
                }
            }
            catch (Exception ex)
            {
                resData.rData["rCode"] = 1;
                resData.rData["rMessage"] = ex.Message;
            }
            return resData;
        } 
          public static bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
        public static bool IsValidMobileNumber(string phoneNumber)
        {
            string pattern = @"^[0-9]{7,15}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }