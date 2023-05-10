using Microsoft.EntityFrameworkCore;

namespace CGTCalculator;

internal class DataSource : DbContext
{
    private DbSet<DatabaseInfo> DatabaseInfos { get; set; } = null!;
    private DbSet<Transaction> Transactions { get; set; } = null!;

    public DataSource(DbContextOptions<DataSource> options)
        : base(options)
    {
    }

    public DatabaseInfo GetDatabaseInfo()
    {
        var databaseInfos = this.DatabaseInfos.ToArray();
        if (databaseInfos is [{ } info])
        {
            return info;
        }

        // If there is more than one, trim it down to one. If there are none, add one.
        DatabaseInfo databaseInfo;
        if (databaseInfos is [{ } firstInfo, .. var rest])
        {
            this.DatabaseInfos.RemoveRange(rest);
            databaseInfo = firstInfo;
        }
        else
        {
            databaseInfo = new DatabaseInfo()
            {
                Version = 1
            };
            this.DatabaseInfos.Add(databaseInfo);
        }

        SaveChanges();
        return databaseInfo;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync()
    {
        return await this.Transactions.ToListAsync().ConfigureAwait(false); ;
    }
}
