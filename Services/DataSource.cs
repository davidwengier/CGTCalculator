﻿using Microsoft.EntityFrameworkCore;

namespace CGTCalculator;

internal class DataSource : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    public DataSource(DbContextOptions<DataSource> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync()
    {
        return await Transactions.ToListAsync();
    }
}