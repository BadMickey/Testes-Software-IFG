using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leilao
{
    public class Startup
    {
        public IConfiguration Configuration { get;}

        public void ConfigurationService(IServiceCollection services)
        {
            //Configurar o DbContext para usar o PostgreSQL
            services.AddDbContext<LeilaoDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            //Registrar o Repositório
            services.AddScoped<ILeilaoRepository, EfLeilaoRepository>();

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<LeilaoService>();

            services.AddControllers();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
