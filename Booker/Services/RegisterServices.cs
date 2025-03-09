using static Booker.Services.SendMailSvc;
using System.Configuration;

namespace Booker.Services
{
    public static partial class RegisterServices
    {
        public static IServiceCollection AddBookerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));           
            services.AddTransient<SendMailSvc>();
            return services;
        }
    }
}
