namespace Chattg
{
    using System;
    using System.Data.Entity;

    public class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
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