using System.Collections.Generic;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Reflection;
using System.IO;
using LMS.Models;
using System;

namespace LMS.Controllers
{
    public class StudentController : Controller
    {
        string DataBase = ConfigurationManager.ConnectionStrings["DataBase"].ConnectionString;
        public ActionResult Index()
        {
            DataTable dataTable = new DataTable();
            DataTable dataTable1 = new DataTable();
            Response.Cache.SetNoStore();
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Student");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql = "select * from " + Session["Course"] + "_" + Session["Branch"] + " where ParentID = 0";
                    using (SqlDataAdapter ad = new SqlDataAdapter(sql, conn))
                    {
                        ad.Fill(dataTable);
                    }
                    string sql1 = "select * from " + Session["Course"] + "_" + Session["Branch"];
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
                return RedirectToAction("Login", "Student");

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
                return RedirectToAction("Login", "Student");
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
                return RedirectToAction("Index", "Student");
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
                return RedirectToAction("Index", "Student");
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql1 = "SELECT * FROM Students WHERE uname = @uname and password = @password";
                    string sql2 = "UPDATE Students SET lastlogin = CURRENT_TIMESTAMP WHERE studentID = @ID";
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
                                Session["Course"] = rd.GetString(6);
                                Session["Branch"] = rd.GetString(7);
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
                        return RedirectToAction("Index", "Student");

                    }
                    else
                    {
                        Session["uname"] = null;
                        Session["uid"] = null;
                        Session["Name"] = null;
                        return RedirectToAction("Login", "Student", new { @loginstatus = false });
                    }
                }

            }
        }

        public ActionResult Logout()
        {
            Session["uname"] = null;
            Session["uid"] = null;
            Session["Name"] = null;
            Session["Roll"] = null;
            Session["Course"] = null;
            Session["Branch"] = null;
            return RedirectToAction("Login", "Student");

        }

        public ActionResult LockScreen()
        {
            Response.Cache.SetNoStore();
            if (Session["uname"] != null)
            {
                return RedirectToAction("Index", "Student");
            }
            else
            {
                return View();
            }
        }

        public ActionResult Subject(int subId, string subName)
        {
            if (Session["uname"] != null)
            {
                List<string> facultyDetials = new List<string>();
                DataTable dataTable = new DataTable();
                DataTable dataTable1 = new DataTable();
                DataTable dataTable2 = new DataTable();
                using(SqlConnection conn = new SqlConnection(DataBase))
                {
                    conn.Open();
                    string sql1 = "select * from Faculty where Subjects like @sub";
                    string sql2 = "select * from Docs_Diploma_CS where SubjectID = @id AND Type = @dType";
                    using (SqlCommand cmd = new SqlCommand(sql1, conn))
                    {
                        cmd.Parameters.AddWithValue("@sub", ("%" + subName + "%"));
                        int n = 0;
                        SqlDataReader rd = cmd.ExecuteReader();
                        while (rd.Read())
                        {
                            n++;
                            facultyDetials.Add(rd.GetString(1));
                            facultyDetials.Add(rd.GetString(9));
                            facultyDetials.Add(rd.GetString(2));
                            facultyDetials.Add(rd.GetString(10));
                            facultyDetials.Add(rd.GetString(11));
                        }
                        rd.Close();
                        if (n > 0)
                        {
                            ViewBag.FacD = "no faculty found";
                        }
                        else
                        {
                            ViewBag.FacD = "faculty found";
                        }
                        ViewData["Faculty"] = facultyDetials;
                    }
                    using (SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", subId);
                        cmd.Parameters.AddWithValue("@dType", "Assignment");
                        SqlDataAdapter ad = new SqlDataAdapter(cmd);
                        ad.Fill(dataTable);
                        ViewData["Assignments"] = dataTable;
                    }
                    using (SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", subId);
                        cmd.Parameters.AddWithValue("@dType", "Notes");
                        SqlDataAdapter ad = new SqlDataAdapter(cmd);
                        ad.Fill(dataTable1);
                        ViewData["Notes"] = dataTable1;
                    }
                    using (SqlCommand cmd = new SqlCommand(sql2, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", subId);
                        cmd.Parameters.AddWithValue("@dType", "Syllabus");
                        SqlDataAdapter ad = new SqlDataAdapter(cmd);
                        ad.Fill(dataTable2);
                        ViewData["Syllabus"] = dataTable2;
                    }

                }
                return View();

            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult Download(string path)
        {
            string fileName = Path.GetFileName(path);
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + (fileName));
            Response.WriteFile(path);
            Response.End();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SubmitAssignment(Asignment model)
        {
            if (model.assgnmentFile != null && model.assgnmentFile.ContentLength > 0)
            {
                try
                {
                    string ext = Path.GetExtension(model.assgnmentFile.FileName);
                    DateTime cur = DateTime.Now;
                    string dtt = cur.Year.ToString() + cur.Month.ToString() + cur.Day.ToString() + cur.Hour.ToString() + cur.Minute.ToString() + cur.Second.ToString();
                    string path = Path.Combine("D:/LMS/Student/Assignment", dtt + ext);

                }
                catch (Exception ex)
                {
                    //
                }
            }
            return View();
        }
    }

}
