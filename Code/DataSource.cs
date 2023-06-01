namespace CGTCalculator;

internal class DataSource : DbContext
{
    public DbSet<DatabaseInfo> DatabaseInfos { get; private set; } = null!;
    public DbSet<Transaction> Transactions { get; private set; } = null!;
    public bool IsInitialized { get; private set; }

    public DataSource(DbContextOptions<DataSource> options)
        : base(options)
    {
    }

    internal void Initialize()
    {
        this.Database.EnsureCreated();
        _ = this.GetDatabaseInfo();
        this.IsInitialized = true;
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
}
