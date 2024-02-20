using System.Reflection;
using Falcata.BillPlaner.Persistence.Context;
using Falcata.BillPlaner.Persistence.Repositories;
using Falcata.BillPlanner.Application.Interfaces.Repositories;
using Falcata.BillPlanner.Domain.Models.BillPlanner.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Falcata.BillPlanner.DI;

[ExcludeFromCodeCoverage]
public static class PersistenceContainers
{
    public static void RegisterPersistenceContainers(this IServiceCollection service, params Assembly[] assemblies)
    {
        RegisterContext(service, assemblies);
        RegisterBillPlanerRepositories(service, assemblies);
    }

    private static void RegisterBillPlanerRepositories(this IServiceCollection service, Assembly[] assemblies)
    {
        //service.AddScoped(typeof(IUserQueryRepository), typeof(UserRepository));
        
        // Register class
        // service.AddScoped<TRepository>()
        service.AddScoped<AccountRepository>();
        service.AddScoped<DebtPeriodRepository>();
        service.AddScoped<AccountMovementRepository>();
            
        // Link class with interfaces
        // service.AddScoped<TIRepository>(x => x.GetService<TRepository>() ?? throw new InvalidOperationException())
        service.AddScoped<IAccountQueryRepository>(x => x.GetService<AccountRepository>() ?? throw new InvalidOperationException());
        service.AddScoped<IAccountCommandRepository>(x => x.GetService<AccountRepository>() ?? throw new InvalidOperationException());
        service.AddScoped<IAccountMovementQueryRepository>(x => x.GetService<AccountMovementRepository>() ?? throw new InvalidOperationException());
        service.AddScoped<IAccountMovementCommandRepository>(x => x.GetService<AccountMovementRepository>() ?? throw new InvalidOperationException());
        service.AddScoped<IDebtPeriodQueryRepository>(x => x.GetService<DebtPeriodRepository>() ?? throw new InvalidOperationException());
        service.AddScoped<IDebtPeriodCommandRepository>(x => x.GetService<DebtPeriodRepository>() ?? throw new InvalidOperationException());
    }

    private static void RegisterContext(this IServiceCollection service, Assembly[] assemblies)
    {
        service.AddScoped<IBillPlanerDbContext>(s =>
        {
            var config = s.GetService<IConfiguration>();
            var connString = config?.GetConnectionString("BillPlanerConnection");
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "Verify if the User Secrets were added correctly");

            var opt = new DbContextOptionsBuilder<BillPlanerDbContext>();
            opt.UseSqlServer(connString);

            var logger = s.GetService<ILogger<BillPlanerDbContext>>();
            if (logger is null)
                throw new ArgumentNullException(nameof(logger), "Verify logger settings");
            opt.LogTo(message => { logger.LogDebug(message); }, LogLevel.Information);
            
            return new BillPlanerDbContext(opt.Options);
        });
    }
}