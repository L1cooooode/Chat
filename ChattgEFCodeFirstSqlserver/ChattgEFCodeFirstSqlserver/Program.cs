using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattgEFCodeFirstSqlserver
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (Model1 db = new Model1()) {
                db.Userinfoes.Add(new Userinfo() { Id = 1, UserNumber = "a12345", UserName = "沉鱼落雁",UserPassword="123456", UserResignDate=DateTime.Now });
                db.SaveChanges();
            }
        }
    }
}