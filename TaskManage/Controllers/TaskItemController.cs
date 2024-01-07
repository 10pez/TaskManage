using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using TaskManage.Data;
using TaskManage.Data.Migrations;
using TaskManage.Models;

namespace TaskManage.Controllers
{
    public class TaskItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskItem
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index()
        {
              return _context.Tasks != null ? 
                          View(await _context.Tasks.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Tasks'  is null.");
        }

        // GET: TaskItem/Details/5
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var taskItem = await _context.Tasks
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // GET: TaskItem/Create
        [Route("Group/{groupId}/TaskItem/Create")]
        public IActionResult Create(int groupId)
        {
            ViewData["GroupId"] = groupId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Group/{groupId}/TaskItem/Create")]
        public async Task<IActionResult> Create([FromRoute] int groupId, [Bind("Id,Name,Description,DueDate,CreatedAt,Priority,IsDone,GroupId")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Group", new { id = groupId });
            }
            ViewData["GroupId"] = groupId;
            return View(taskItem);
        }

        // GET: TaskItem/Edit/5
        [Route("Group/{groupId}/TaskItem/Edit/{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            return View(taskItem);
        }

        // POST: TaskItem/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Group/{groupId}/TaskItem/Edit/{id}")]
        [Authorize(Roles = "Admin,Manager,User")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DueDate,CreatedAt,Priority,IsDone,GroupId")] TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taskItem);
        }

        // GET: TaskItem/Delete/5
        [Route("Group/{groupId}/TaskItem/Delete/{id}")]
        [Authorize(Roles = "Admin,Manager,User")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var taskItem = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: TaskItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("Group/{groupId}/TaskItem/Delete/{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int groupId, int id)
        {
            if (_context.Tasks == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tasks'  is null.");
            }
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem != null)
            {
                _context.Tasks.Remove(taskItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Group", new { id = groupId });


        }

        [Route("Group/{groupId}/TaskItem/Complete/{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> CheckAsComplete([FromRoute]int groupId, int id)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            taskItem.IsDone = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Group", new { id = groupId });

        }

        [Route("Group/{groupId}/TaskItem/Uncomplete/{id}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> CheckAsUncompleted([FromRoute] int groupId, int id)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }

            taskItem.IsDone = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Group", new { id = groupId });

        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> AddComment(int taskId, string commentText)
        {
            var taskItem = await _context.Tasks.FindAsync(taskId);
            if (taskItem == null)
            {
                return NotFound();
            }

            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var comment = new Comment
            {
                TaskItemId = taskId,
                Content = commentText,
                UserId = userId,
                UserName = userName,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = taskId });

        }

        private bool TaskItemExists(int id)
        {
          return (_context.Tasks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
