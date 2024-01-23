using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using OtpGenerationApi.services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Authentication;


// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();


WebHost.CreateDefaultBuilder().
ConfigureServices(s=>
{
    IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    s.AddSingleton<Login>();
  s.AddSingleton<OtpGeneration>();
s.AddAuthorization();
s.AddControllers();
s.AddCors();
s.AddAuthentication("SourceJWT").AddScheme<SourceJwtAuthenticationSchemeOptions, SourceJwtAuthenticationHandler>("SourceJWT", options =>
    {
        options.SecretKey = appsettings["jwt_config:Key"].ToString();
        options.ValidIssuer = appsettings["jwt_config:Issuer"].ToString();
        options.ValidAudience = appsettings["jwt_config:Audience"].ToString();
        options.Subject = appsettings["jwt_config:Subject"].ToString();
    });

}).Configure(app=>
{
// app.UseAuthentication();
// app.UseAuthorization();
 app.UseCors(options =>
         options.WithOrigins("https://localhost:5002", "http://localhost:5001")
        
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.UseEndpoints(e=>
{

  var login=  e.ServiceProvider.GetRequiredService<Login>();
 var forgetpassword =e.ServiceProvider.GetRequiredService<OtpGeneration>(); 
//   var forgetPasswordService = e.RequestServices.GetRequiredService<IForgetPasswordService>();
    e.MapPost("login",
        [AllowAnonymous] async (HttpContext http) =>
         {
             var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
             requestData rData = JsonSerializer.Deserialize<requestData>(body);
              if (rData.eventID == "1001") // update
                         await http.Response.WriteAsJsonAsync(await login.LoginData(rData));

         });

    //         e.MapPost("forgetpassword",
    //   [AllowAnonymous] async (HttpContext http) =>
    //      {
    //          var body = await new StreamReader(http.Request.Body).ReadToEndAsync();
    //          requestData rData = JsonSerializer.Deserialize<requestData>(body);
    //           if (rData.eventID == "1001")
    //                      await http.Response.WriteAsJsonAsync(await forgetpassword.PasswordForget(rData));

    //      });
      
    e.MapGet("/test-endpoint",
    async context =>
    {
        try
        {
            // Create a requestData object for testing
            var testData = new requestData
            {
                eventID = "1001",
                addInfo = new Dictionary<string, object>
                {
                    { "U_Mobile", "8000489287" },
                    { "New_Password", "testPassword123" },
                    { "guid","4C4C4544-0053-4C10-804D-B3C04F575433"}
                    // Add other key-value pairs as needed
                }
            };

            // Your existing logic for handling requestData, perform operations, etc.

            // Create a responseData object for the response
            var response = new responseData
            {
                rStatus = 0,
                eventID = testData.eventID,
                addInfo = testData.addInfo,
                rData = new Dictionary<string, object>
                {
                    // Add response data as needed
                    { "Message", "Test successful" }
                }
            };

            // Respond with the result as JSON
            await context.Response.WriteAsJsonAsync(await forgetpassword.PasswordForget(testData));
        }
        catch (Exception ex)
        {
            // Handle exceptions and respond with an error
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new responseData
            {
                rStatus = 1,
                rData = new Dictionary<string, object>
                {
                    { "ErrorMessage", ex.Message }
                }
            });
        }
    });


    });

}).Build().Run();


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

public record requestData
{
    [Required]
    public string eventID { get; set; }
    [Required]
  
public IDictionary<string, object> addInfo { get; set; }
   
}

public record responseData
{
    public responseData()
    {
        eventID = "";
        rStatus = 0;
        rData = new Dictionary<string, object>();
    }
    [Required]
    public int rStatus { get; set; } = 0;
    public string eventID { get; set; }
    
    public IDictionary<string, object> addInfo { get; set; }
   
    public IDictionary<string, object> rData { get; set; }
}

