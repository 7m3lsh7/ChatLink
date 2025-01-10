using Microsoft.AspNetCore.Mvc;
using webchat.data;

namespace webchat.Controllers
{
    public class LoginController : Controller
    {
        private readonly ChatDbcontect _chatDbcontect;
        public LoginController(ChatDbcontect chatDbcontect)
        {
            _chatDbcontect = chatDbcontect;
        }

        public IActionResult Index()
        {
            ViewData["HideNavbar"] = true;
            ViewData["HideFooter"] = true;
            return View();
        }

       [HttpPost]
       [ValidateAntiForgeryToken]
       public IActionResult Check(User user)
        {
          var response = _chatDbcontect.users.FirstOrDefault(u => u.Email == user.Email && u.PasswordHash == user.PasswordHash);
            if (response == null)
            {
                return NotFound();
            }
            else
            {
                return RedirectToAction("ViewUser", "Login");
            }
        }
        

        public IActionResult ViewUser()
        {
            var response = _chatDbcontect.users.ToList();
            return View(response);
        }
       
        public IActionResult Delete(int id)
        {
            var response = _chatDbcontect.users.Find(id);
            _chatDbcontect.users.Remove(response);
            _chatDbcontect.SaveChanges();
            return RedirectToAction("ViewUser", "Login");
        }
                  
    }                      
}

/*
 <nav class="navbar navbar-expand-lg color">
        <div class="container-fluid">
            <div class="navbar-column">
                <div class="profile-picture">
                <img src="https://via.placeholder.com/150" alt="Profile Picture">
            </div>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
            <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
            <ul class="navbar-nav">
                <li class="nav-item pt-2 pb-2">
                <a class="nav-link active" aria-current="page" href="./Index.html">
                    <i class="fa-solid text-white fa-house"></i>
                </a>
                </li>
                <li class="nav-item pt-2 pb-2">
                <a class="nav-link" href="./chat.html">
                    <i class="fa-solid text-white fa-bell"></i>
                </a>
                </li>
                <li class="nav-item pt-2 pb-2">
                <a class="nav-link" href="#">
                    <i class="fa-solid text-white fa-message"></i>
                </a>
                </li>
                <li class="nav-item pt-2 pb-2">
                <a class="nav-link">
                    <i class="fa-solid text-white fa-sliders"></i>
                </a>
                </li>
            </ul>
            </div>
            <div class="logout">
                <ul class="navbar-nav">
                    <li class="nav-item">
                        <a class="nav-link"  href="#">
                            <i class="fa-solid  text-white fa-right-from-bracket"></i>
                        </a>
                    </li>
                </ul>
            </div>
            </div>
        </div>
    </nav>
 */
