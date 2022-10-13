using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using LMS.Models;
using System.IO;
using static System.Net.WebRequestMethods;

namespace LMS.Controllers
{
    public class AdminController : Controller
    {
        string DataBase = ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString.ToString();
        // GET: Admin
        [HttpGet]
        public ActionResult Login(bool loginstatus = true)
        {
            Response.Cache.SetNoStore();
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
            if (Session["uid"] != null && Session["FName"] != null)
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
            Response.Cache.SetNoStore();
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
            Response.Cache.SetNoStore();
            return View();
        }

        public ActionResult FileUpload()
        {
            if (Session["uid"] == null && Session["FName"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {

                DataTable dataTable = new DataTable();
                DataTable dataTable1 = new DataTable();
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql = "select * from Diploma_CS where ParentID = 0";
                    using (SqlDataAdapter ad = new SqlDataAdapter(sql, conn))
                    {
                        ad.Fill(dataTable);
                    }
                    string sql1 = "select * from Diploma_CS";
                    using (SqlDataAdapter ad = new SqlDataAdapter(sql1, conn))
                    {
                        ad.Fill(dataTable1);
                    }
                }
                Session["subt"] = dataTable;
                Session["subt1"] = dataTable1;

                return View();
            }

        }
        [HttpPost]
        public ActionResult FileUpload(Uploader model)
        {

            if (model.filee != null && model.filee.ContentLength > 0)
            {
                try
                {
                    string ext = Path.GetExtension(model.filee.FileName);
                    DateTime cur = DateTime.Now;
                    string dtt = cur.Year.ToString() + cur.Month.ToString() + cur.Day.ToString() + cur.Hour.ToString() + cur.Minute.ToString() + cur.Second.ToString();
                    string path = Path.Combine("D:/LMS", model.upload_type, dtt + "_" + model.title.Replace(' ', '_').Trim() + ext);

                    int parentI = 0;

                    using (SqlConnection conn = new SqlConnection(DataBase))
                    {
                        conn.Open();
                        string sql1 = "SELECT Id FROM Diploma_CS WHERE Name = @subject";

                        string sql2 = "INSERT INTO Docs_Diploma_CS ([Title], [Type], [Path], [SubjectID], [uploadedOn], [uploadedBy]) " +
                            "VALUES (@title, @type, @path, @id, CURRENT_TIMESTAMP, '"+ Session["FName"] +"')";
                        using (SqlCommand cmd = new SqlCommand(sql1, conn))
                        {
                            cmd.Parameters.AddWithValue("@subject", model.subb);
                            SqlDataReader rd = cmd.ExecuteReader();
                            while (rd.Read())
                            {
                                parentI = rd.GetInt32(0);
                            }
                            rd.Close();
                        }
                        if (parentI > 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(sql2, conn))
                            {
                                cmd.Parameters.AddWithValue("@title", model.title);
                                cmd.Parameters.AddWithValue("@type", model.upload_type);
                                cmd.Parameters.AddWithValue("@path", path);
                                cmd.Parameters.AddWithValue("@id", parentI);

                                cmd.ExecuteNonQuery();
                            }
                            model.filee.SaveAs(path);
                            ViewBag.UploadSatatus = "success";
                        }
                        else
                        {
                            ViewBag.UploadSatatus = "error";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.UploadSatatus = "error";
                }
            }
            else
            {
                ViewBag.UploadSatatus = "nofile";
            }
            return View();
        }
    }
}
