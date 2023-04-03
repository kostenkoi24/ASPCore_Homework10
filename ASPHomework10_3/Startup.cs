using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPHomework10_3
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            //services.AddTransient<MiddlwareException>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) //, ILoggerFactory loggerFactory
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            
            app.UseMiddleware<HttpLoggingMiddleware>();

            

        }
    }


    public class HttpLoggingMiddleware //скозлил пример в интернете :)
        {
            private readonly ILogger _logger;
            private readonly RequestDelegate _next;

            public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
            {
                _next = next;
                _logger = logger;
                _logger.LogError(new NullReferenceException(), "My Logged exception from HttpLoggingMiddleware!!!!!!!!!!!! method");
            }

            public async Task Invoke(HttpContext context)
            {
                //Copy  pointer to the original response body stream
                var originalBodyStream = context.Response.Body;

                //Get incoming request
                var request = await GetRequestAsTextAsync(context.Request);
                //Log it
                _logger.LogInformation(request);


                //Create a new memory stream and use it for the temp reponse body
                await using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                //Continue down the Middleware pipeline
                await _next(context);

                //Format the response from the server
                var response = await GetResponseAsTextAsync(context.Response);
                //Log it
                _logger.LogInformation(response);

                //Copy the contents of the new memory stream, which contains the response to the original stream, which is then returned to the client.
                await responseBody.CopyToAsync(originalBodyStream);
            }


            private async Task<string> GetRequestAsTextAsync(HttpRequest request)
            {
                var body = request.Body;

                //Set the reader for the request back at the beginning of its stream.
                request.EnableBuffering();

                //Read request stream
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];

                //Copy into  buffer.
                await request.Body.ReadAsync(buffer, 0, buffer.Length);

                //Convert the byte[] into a string using UTF8 encoding...
                var bodyAsText = Encoding.UTF8.GetString(buffer);

                //Assign the read body back to the request body
                request.Body = body;

                return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
            }

            private async Task<string> GetResponseAsTextAsync(HttpResponse response)
            {
                response.Body.Seek(0, SeekOrigin.Begin);
                //Create stream reader to write entire stream
                var text = await new StreamReader(response.Body).ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);

                return text;
            }
        }

    
}
