using ASPHomework10_2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ASPHomework10_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IOptions<MailServer> mailServer)
        {
            _logger = logger;
            MailServer = mailServer;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IOptions<MailServer> MailServer { get; }

        [HttpPost]
        public IActionResult Index(EmailMessageModel model)
        {
            
            var smtpClient = new SmtpClient(MailServer.Value.ServerName)
            {
                Port = MailServer.Value.PortNumber,
                Credentials = new NetworkCredential(MailServer.Value.Sender, ""),
                EnableSsl = true,
            };

            smtpClient.Send(MailServer.Value.Sender, model.Email, model.Subject, model.Body);


            return View("Success");
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
