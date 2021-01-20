using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNBase.Models;
using Microsoft.EntityFrameworkCore;

namespace DNBase.Data
{
    public class DNBaseContext : DbContext
    {
        public DNBaseContext(DbContextOptions<DNBaseContext> options) : base(options)
        {
        }

        public DbSet<SubgenreModel> Subgenres { get; set; }
        public DbSet<ArtistModel> Artists { get; set; }
        public DbSet<AlbumModel> Albums { get; set; }
        public DbSet<TrackModel> Tracks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubgenreModel>().ToTable("Subgenre");
            modelBuilder.Entity<ArtistModel>().ToTable("Artist");
            modelBuilder.Entity<AlbumModel>().ToTable("Album");
            modelBuilder.Entity<TrackModel>().ToTable("Track");
        }
    }
}
