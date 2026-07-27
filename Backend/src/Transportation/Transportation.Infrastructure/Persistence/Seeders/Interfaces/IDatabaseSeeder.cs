namespace Transportation.Infrastructure.Persistence.Seeders.Interfaces;

public interface IDatabaseSeeder
{
    int Order { get; }
    Task SeedAsync();
}

public interface IBlankDataSeeder : IDatabaseSeeder;

public interface IDemoDataSeeder : IDatabaseSeeder;