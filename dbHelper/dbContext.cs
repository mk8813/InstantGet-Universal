using Microsoft.EntityFrameworkCore;


namespace DbHelper
{
    public class dbHelperConnection : DbContext
    {
        public DbSet<tbl_History> tbl_History { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=db.sqlite");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<tbl_History>()
                .Property(b => b.Id)
                .IsRequired();

        }



    }

    //////////////////////////////////////////////////////////////////////
    public class tbl_History
    {
        //The Id property is marked as the Primary Key   

        public int Id { get; set; }//2132132
        public string Url { get; set; }//instageam......
        public string Type { get; set; }//picture / video
        public string SavePath { get; set; } //c:\\aaaaaaaaa
        public string DateInserted { get; set; } // datetime
        public string IsDownloaded { get; set; } // 1 : yes - 0 : no
        public string DownloadGuid { get; set; }//Guid od download

    }
}
