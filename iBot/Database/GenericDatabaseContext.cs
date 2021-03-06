﻿using System;
using System.Data.Entity;
using System.Data.SQLite;
using NLog;
using SQLite.CodeFirst;

namespace IBot.Database
{
    internal class GenericDatabaseContext<T> : DbContext
        where T : class
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public GenericDatabaseContext()
            : base(new SQLiteConnection()
            {
                ConnectionString = new SQLiteConnectionStringBuilder()
                {
                    // ReSharper disable once UseStringInterpolation
                    DataSource = string.Format("{0}/iBot/gstore.{1}.db3",
                                               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                               typeof(T).GUID.ToString("N")),
                    ForeignKeys = true,
                    Password = "secret",
                }.ConnectionString
            }, true) {}

        public DbSet<T> Table { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            try
            {
                var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<GenericDatabaseContext<T>>(modelBuilder);
                System.Data.Entity.Database.SetInitializer(sqliteConnectionInitializer);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
