using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace MediaNest.ApiService.Database; 
public static class MongoDbServiceExtensions {

    /// <summary>
    /// call after builder.Services.Configure<MongoDbConfig>(...)
    /// </summary>
    public static IServiceCollection AddMongoClient(this IServiceCollection services) {
        
        services.AddSingleton<IMongoClient>(sp => { 
            var config = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
            return new MongoClient(config.ConnectionString);
        });
        return services;
    }
    /// <summary>
    /// call after AddMongoClient
    /// </summary>
    public static IServiceCollection AddMongoCollection<T>(this IServiceCollection services, string collectionName) {
        services.AddScoped(sp => {
            var config = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(config.DatabaseName);
            var collection = database.GetCollection<T>(collectionName);
            return collection;
        });
        return services;
    }
}
