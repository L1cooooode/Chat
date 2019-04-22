namespace ChattgEFCodeFirstSqlserver
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class Model2 : DbContext
    {
        public Model2()
            : base("name=Model2")
        {
        }

         public virtual DbSet<Userinfo> Userinfoes { get; set; }
    }
    public class Userinfo
    {
        public int Id { get; set; }
        public string UserNumber { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public DateTime UserResignDate { get; set; }
    }
}