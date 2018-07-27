using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RawSql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RawSql.Controllers
{
    public class MemberController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            bool loggedIn = await checkLoggedIn();
            if (loggedIn) return RedirectToAction(nameof(Success));
            else return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(Member member)
        {
            if (ModelState.IsValid)
            {
                MemberContext context = HttpContext.RequestServices.GetService(typeof(Models.MemberContext)) as MemberContext;
                bool verify = await context.VerifyPassword(member.Username, member.Password);

                if (verify)
                {
                    string session_id = await context.WriteSession(member.Username);
                    HttpContext.Session.SetString("_sessionid", session_id);
                    HttpContext.Session.SetString("_username", member.Username);

                    return RedirectToAction(nameof(Success));
                }
                else
                {
                    return RedirectToAction(nameof(Fail));
                }

            }

            return RedirectToAction(nameof(Fail));
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Fail()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Member member)
        {
            MemberContext context = HttpContext.RequestServices.GetService(typeof(Models.MemberContext)) as MemberContext;
            if(ModelState.IsValid)
            {
                await context.CreateUser(member);
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            MemberContext context = HttpContext.RequestServices.GetService(typeof(Models.MemberContext)) as MemberContext;
            List<Member> members = await context.GetMembers();
            return View(members);
        }
        
        private async Task<bool> checkLoggedIn()
        {
            var username = HttpContext.Session.GetString("_username");
            if (username == null) return false;

            var session_id = HttpContext.Session.GetString("_sessionid");

            MemberContext context = HttpContext.RequestServices.GetService(typeof(Models.MemberContext)) as MemberContext;
            if (await context.VerifySession(username, session_id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
