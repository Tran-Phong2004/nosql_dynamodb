
namespace QLDonHang.DynamoDB.Seed
{
    public class DbSeedHosted : IHostedService
    {
        private readonly DbSeeder _seeder;
        public DbSeedHosted(DbSeeder dbSeeder)
        {
            _seeder = dbSeeder;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _seeder.SeedAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
