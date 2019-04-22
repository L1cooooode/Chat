using System;
using System.Linq;

namespace Chattg
{
    public class CURD
    {
        public void Addmessage(string addUserNumber, string addUserName, string addUserPassword)//增
        {
            using (Model1 db = new Model1())
            {
                db.Userinfoes.Add(new Userinfo()
                {
                    UserNumber = addUserNumber,
                    UserName = addUserName,
                    UserPassword = addUserPassword,
                    UserResignDate = DateTime.Now
                });//无则增（有则改）

                db.SaveChanges();
            }
        }
        public void Deletemessage(int addId, string addUserNumber, string addUserName, string addUserPassword)
        {
            using (Model1 db = new Model1())
            {
                db.Userinfoes.Add(new Userinfo() { Id = addId, UserNumber = addUserNumber, UserName = addUserName, UserResignDate = DateTime.Now });//无则增（有则改）

                var person = db.Userinfoes.Find(3);//查找主键PersonID=1, PersonName="Michael"的实体

                // Userinfo aDShiTi = db.Userinfoes.Find(1);//查

                Userinfo aDda2 = db.Userinfoes.FirstOrDefault(p => p.UserName == addUserName);

                if (aDda2 == null) { return; }
                db.Userinfoes.Remove(aDda2);//删

                db.SaveChanges();
            }
        }
        public string Searchallmessage()
        {
            string mesg="";
            using (Model1 db = new Model1())
            {
                var query = from s in db.Userinfoes
                            select s;
                foreach (var item in query)
                {
                    Console.WriteLine($"ID={item.Id}||登录账号={item.UserNumber}||用户名={item.UserName}||登录密码={item.UserPassword}||注册日期={item.UserResignDate}");
                    mesg+= ($"ID={item.Id}||登录账号={item.UserNumber}||用户名={item.UserName}||登录密码={item.UserPassword}||注册日期={item.UserResignDate}\n");
                }
            }
            return mesg;
        }
        public bool SearchNumber(string addUserNumber)//查询账号是否存在
        {
            using (Model1 db = new Model1())
            {
                Userinfo aDda2 = db.Userinfoes.FirstOrDefault(p => p.UserNumber == addUserNumber);
                if (aDda2 == null)
                 return false; //不存在
            }
            return true;
        }
        public string SearchName(string addUserNumber)//查询该账号的昵称
        {
            using (Model1 db = new Model1())
            {
                Userinfo aDda2 = db.Userinfoes.FirstOrDefault(p => p.UserNumber == addUserNumber);
                return aDda2.UserName;
            }
        }
        public bool SearchPassword(string addUserNumber,string addUserPassword)//查询密码是否匹配
        {
            using (Model1 db = new Model1())
            {
                Userinfo aDda2 = db.Userinfoes.FirstOrDefault(p => p.UserNumber == addUserNumber);
                if (aDda2.UserPassword == addUserPassword)
                { return true; }
            }
            return false;
        }

        public void Searchmessage(int addId, string addUserNumber, string addUserName, string addUserPassword)
        {
            using (Model1 db = new Model1())
            {
                var query = from s in db.Userinfoes
                            select s;

                foreach (var item in query)
                {
                    Console.WriteLine(item.Id + ":" + item.UserName+":"+item.UserPassword);
                }

                    //var person = db.Userinfoes.Find(3);//查找主键PersonID=1, PersonName="Michael"的实体
                    // Userinfo aDShiTi = db.Userinfoes.Find(1);//查
                    //Userinfo aDda2 = db.Userinfoes.FirstOrDefault(p => p.UserName == addUserName);//查
                }

        }

        public void Changemessage(int addId, string addUserNumber, string addUserName, string addUserPassword)
        {

        }
    }
}
