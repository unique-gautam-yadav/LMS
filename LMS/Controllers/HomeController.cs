using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using LMS.Models;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace LMS.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString;
        public ActionResult Index()
        {
            DataTable dataTable = new DataTable();
            DataTable dataTable1 = new DataTable();
            Response.Cache.SetNoStore();
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "select * from SliderCont where ParentID = 0";
                    using (SqlDataAdapter ad = new SqlDataAdapter(sql, conn))
                    {
                        ad.Fill(dataTable);
                    }
                    string sql1 = "select * from SliderCont";
                    using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                    {
                        ad.Fill(dataTable1);
                    }
                }
                Session.Add("sliderItems", dataTable);
                Session.Add("sliderItems1", dataTable1);
                return View();
            }
        }

        public ActionResult About()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login");

            }
            else
            {
                ViewBag.Message = "Your application description page.";
                return View();
            }
        }

        public ActionResult Contact()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.Message = "Your contact page.";
                return View();

            }
        }
        public ActionResult Login(bool loginstatus = true)
        {
            Response.Cache.SetNoStore();
            if (Session["uname"] != null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(loginstatus);
            }

        }

        [HttpPost]
        public ActionResult Login(FormCollection collection, Models.Login log)
        {
            if (Session["uname"] != null)
             {
                 return RedirectToAction("Index");
             }
             else
             {
                 using (SqlConnection conn = new SqlConnection(connectionString))
                 {
                     conn.Open();
                     string sql1 = "SELECT * FROM users WHERE uname = @uname and password = @password";
                     string sql2 = "UPDATE users SET lastlogin = CURRENT_TIMESTAMP WHERE ID = @ID";
                     int n = 0;
                     using (SqlCommand cmd = new SqlCommand(sql1, conn))
                     {
                         cmd.Parameters.AddWithValue("@uname", log.uname);
                         cmd.Parameters.AddWithValue("@password", log.password);
                         SqlDataReader rd = cmd.ExecuteReader();
                         while (rd.Read())
                         {
                             if (rd.GetString(2) == log.uname && rd.GetString(4) == log.password)
                             {
                                 Session["uname"] = rd.GetString(2);
                                 Session["Name"] = rd.GetString(3);
                                 Session["ID"] = rd.GetInt32(0);
                                Session["Roll"] = rd.GetInt32(1);
                                 n++;
                             }
                         }
                         rd.Close();

                     }
                     if (n != 0)
                     {
                         using(SqlCommand cmd = new SqlCommand(sql2, conn))
                         {
                             cmd.Parameters.AddWithValue("@ID", Session["ID"]);
                             cmd.ExecuteNonQuery();
                         }
                         Response.Write(log.remember);
                         return RedirectToAction("Index");

                     }
                     else
                     {
                         Session["uname"] = null;
                         Session["uid"] = null;
                         Session["Name"] = null;
                         return RedirectToAction("Login", new {@loginstatus = false});
                     }
                 }
               
        }
        }

        public ActionResult Logout()
        {
            Session["uname"] = null;
            Session["uid"] = null;
            Session["Full Name"] = null;
            return RedirectToAction("Login");

        }

        public ActionResult LockScreen()
        {
            Response.Cache.SetNoStore();
            if (Session["uname"] != null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult Admin() => View();
        
    }

}
