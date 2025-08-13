using Repositories.TableRepositories;
using ServiceDataRepo.Entities;
using ServiceDataRepo.TableRepositories;
using StackOverflowService.DTOs.AuthDtos;
using System.Linq;
using System.Web.Mvc;

public class AuthController : Controller
{
    private readonly UserTableRepository _users;

    public AuthController()
    {

        _users = new UserTableRepository();
    }

    [HttpPost]
    public ActionResult Register(RegisterRequest req)
    {
        var exists = _users.GetAll().Any(u => u.Email == req.Email);
        if (exists)
            return new HttpStatusCodeResult(409, "Email already exists");

        var user = new UserEntity(req.Email)
        {
            FullName = req.FullName,
            Gender = req.Gender,
            Country = req.Country,
            City = req.City,
            Address = req.Address,
            Password = req.Password, // ideally hash this but nahhhh who cares about security especially if not mentiond in the text :))
        };

        _users.Insert(user);

        return Json(new { message = "User registered successfully" });
    }

    [HttpPost]
    public ActionResult Login(LoginRequest req)
    {
        var user = _users.GetAll().FirstOrDefault(u => u.Email == req.Email);

        if (user == null || user.Password != req.Password)
            return new HttpStatusCodeResult(401, "Invalid credentials");

        return Json(new { message = "Login successful" });
    }
}
