﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StackOverflowService.Controllers
{
    public class AnswerController : Controller
    {
        // GET: Answer
        public ActionResult Index()
        {
            return View();
        }
    }
}