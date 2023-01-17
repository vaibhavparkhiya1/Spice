﻿

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using System.Security.Claims;

namespace Spice.ViewComponenets
{
    public class UserNameViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public UserNameViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity  = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            var userFromDb = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == claims.Value);

            return View (userFromDb);
        }

    }
}