﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCDT52CW2Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCDT52CW2.Areas.Admin.Controllers
{
    //Role Authorisation Statement goes here...
    [Area("Admin")]
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserRolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            //Injecting Dependencies into the controller... 
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //Index Controller gets all of the available users and returns as a List... 
            var users = await _userManager.Users.ToListAsync();
            var userRolesVMList = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var currentVM = new UserRolesViewModel()
                {
                    UserId = user.Id,
                    EmailAddr = user.Email,
                    RoleTitle = new List<string>(await _userManager.GetRolesAsync(user))
                };
                userRolesVMList.Add(currentVM);
            }
            //This obtains the relevant details from the UserModel, we don't need to see passwords and other fields in this element...
            return View(userRolesVMList);
        }

        //GET
        [HttpGet]
        public async Task<IActionResult> Manage(string userId)
        {
            ViewBag.userId = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return NotFound();
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.InRole = true;
                }
                else
                {
                    userRolesViewModel.InRole = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }

        //POST
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.InRole).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }

    }
}
