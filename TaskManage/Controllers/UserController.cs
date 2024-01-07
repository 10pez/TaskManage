using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManage.Data;
using TaskManage.Models;

[Authorize(Roles = "Admin, Manager")]
public class UserController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;


    public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;

    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        ViewData["Groups"] = _context.Groups.ToList();
        ViewData["Roles"] = _roleManager.Roles.ToList();

        Dictionary<string, IList<string>> userRolesDictionary = new Dictionary<string, IList<string>>();

        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            userRolesDictionary.Add(user.Id, userRoles);
        }

        ViewData["UserRoles"] = userRolesDictionary;

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> SetGroup(string userId, int groupId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        user.GroupId = groupId;

        var userRoles = await _userManager.GetRolesAsync(user);

        // Pobierz role do zachowania (te, które nie zaczynają się od "Group_")
        var rolesToKeep = userRoles.Where(r => !r.StartsWith("Group_"));

        // Usuń użytkownika z pozostałych ról
        await _userManager.RemoveFromRolesAsync(user, userRoles.Except(rolesToKeep));

        // Dodaj nową rolę związaną z grupą
        var newRoleName = "Group_" + _context.Groups.Find(groupId)?.Name;
        var roleExists = await _roleManager.RoleExistsAsync(newRoleName);

        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(newRoleName));
        }

        await _userManager.AddToRoleAsync(user, newRoleName);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }


    [HttpPost]
    public async Task<IActionResult> SetRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        // Sprawdź, czy rola istnieje
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            return NotFound();
        }

        // Dodaj użytkownika do roli
        await _userManager.AddToRoleAsync(user, roleName);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var role = await _roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            return NotFound();
        }

        await _userManager.RemoveFromRoleAsync(user, roleName);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}