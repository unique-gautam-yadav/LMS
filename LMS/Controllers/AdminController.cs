using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace LMS.Controllers
{
    public class AdminController : Controller
    {
        string DataBase = ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString.ToString();
        // GET: Admin
        [HttpGet]
        public ActionResult Login(bool loginstatus = true)
        {
            if (Session["uid"] != null && Session["FName"] != null)
            {
                return RedirectToAction("Index");
            }
            else
            {
            Response.Cache.SetNoStore();
            return View(loginstatus);
            }
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection, Models.Login log)
        {
            if (Session["uname"] != null && Session["FName"] != null)
            { 
                return RedirectToAction("Index");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql1 = "SELECT * FROM Faculty WHERE UID = @uid and Password = @password";
                    string sql2 = "UPDATE Faculty SET lastlogin = CURRENT_TIMESTAMP WHERE ID = @ID";
                    int n = 0;
                    using (SqlCommand cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", log.uname);
                        cmd.Parameters.AddWithValue("@password", log.password);
                        SqlDataReader rd = cmd.ExecuteReader();
                        while (rd.Read())
                        {
                            if (rd.GetString(2) == log.uname && rd.GetString(3) == log.password)
                            {
                                Session["uid"] = rd.GetString(2);
                                Session["FName"] = rd.GetString(1);
                                Session["ID"] = rd.GetInt32(0);
                                Session["EID"] = rd.GetInt32(4);
                                n++;
                            }
                        }
                        rd.Close();

                    }
                    if (n != 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(sql2, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", Session["ID"]);
                            cmd.ExecuteNonQuery();
                        }
                        Response.Write(log.remember);
                        return RedirectToAction("Index");

                    }
                    else
                    {
                        Session["uid"] = null;
                        Session["ID"] = null;
                        Session["FName"] = null;
                        Session["EID"] = null;
                        return RedirectToAction("Login", new { @loginstatus = false });
                    }
                }

            }
        }

        public ActionResult Index()
        {
            if (Session["uid"] == null && Session["FName"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Response.Cache.SetNoStore();
                return View();

            }
        }

        public ActionResult Logout()
        {
            Session["uid"] = null;
            Session["ID"] = null;
            Session["FName"] = null;
            Session["EID"] = null;
            return RedirectToAction("Login");
        }

        public ActionResult Lockscreen()
        {
            return View();
        }

        public ActionResult FileUpload()
        {
            return View();
        }
    }
}
