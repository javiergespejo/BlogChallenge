using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogChallenge.Data;
using BlogChallenge.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace BlogChallenge.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PostsController(IWebHostEnvironment hostEnvironment, ApplicationDbContext context)
        {
            this._hostEnvironment = hostEnvironment;
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index(string sortOrder)
        {

            var posts = await _context.Post.OrderByDescending(p => p.CreationDate).ToListAsync();

            return View(posts);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return View("Error");
            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                ShowError("view");
                return View("Error");
            }
            var category = _context.Post.Where(p => p.Id == id).Select(p => p.Category.Name).FirstOrDefault();
            ViewBag.category = category;
            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            return View(new Post { CreationDate = DateTime.Now });
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Image,ImageName,CategoryId")] Post post)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    post.CreationDate = DateTime.UtcNow;
                    //Save image to wwwroot/image
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(post.Image.FileName);
                    string extension = Path.GetExtension(post.Image.FileName);
                    post.ImageName = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(wwwRootPath + "/Image/", fileName);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await post.Image.CopyToAsync(fileStream);
                    }
                    
                    _context.Add(post);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }

                ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", post.CategoryId);

                return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                ShowError("edit");
                return View("Error");
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Image,ImagenName,CategoryId")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postDate = await _context.Post.FindAsync(id);
                    post.CreationDate = postDate.CreationDate;
                    _context.Entry(postDate).State = EntityState.Detached;

                    //Save image to wwwroot/image                    
                    if (post.Image != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Path.GetFileNameWithoutExtension(post.Image.FileName);
                        string extension = Path.GetExtension(post.Image.FileName);
                        post.ImageName = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath + "/Image/", fileName);
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await post.Image.CopyToAsync(fileStream);
                        }
                    }
                    
                    _context.Post.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        ShowError("edit");
                        return View("Error");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", post.CategoryId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                ShowError("delete");
                return View("Error");
            }

            var category = _context.Post.Where(p => p.Id == id).Select(p => p.Category.Name).FirstOrDefault();
            ViewBag.category = category;

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Post.FindAsync(id);

            if (post == null)
            {
                ShowError("delete");
                return View("Error");
            }

            //Delete imiage from wwwroot/image
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "image", post.ImageName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            //Delete the record
            _context.Post.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.Id == id);
        }

        private void ShowError(string actionDetail)
        {
            ViewBag.ErrorTitle = "The post was not found";
            ViewBag.ErrorMessage = $"The post you want to {actionDetail} was deleted or no longer exists." +
                "Check the URL post.";
        }
    }
}
