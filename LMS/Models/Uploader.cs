﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class Uploader
    {
        public string yrr { get; set; }
        public string semm { get; set; }
        public string subb { get;  set;}
        public string upload_type { get; set; }
        public HttpPostedFileBase filee { get; set; }
    }
}