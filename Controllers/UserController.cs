
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.interfaces;
using Project.Models;
using IAuthorizationService = Project.interfaces.IAuthorizationService;
namespace Project.Controllers;
[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController(IUserService iUserService, IAuthorizationService iAuthorizationService) : ControllerBase
{  
    private IUserService iUserService = iUserService;
    private IAuthorizationService iAuthorizationService=iAuthorizationService;
    //פונקציה לקבלת רשימת הנתונים
    [HttpGet]
    [Authorize (Policy="Admin")]
    public ActionResult<List<User>> Get()
    {  
     return iUserService.GetAllList();
    }
    //id-פונקציה לקבלת אוביקט לפי 
    [HttpGet("{id}")]
    [Authorize]
    public ActionResult<User> Get(int id)
    {
        (string type,int userId)=iAuthorizationService.GetUserClaims(User);
        if (iAuthorizationService.IsAccessDenied(id,type,userId))
                return Unauthorized("Unauthorized: You don't have permission to perform this action.");
        User? user = iUserService.GetUserById(id);
        if (user == null)
            return BadRequest("Invalid id");
        return user;
    }
    //מכניס אוביקט חדש לרשימה
    [HttpPost]
    [Authorize(Policy = "Admin")]
    public ActionResult Create( User newUser)
    {
     (string type,int userId)=iAuthorizationService.GetUserClaims(User);
        if (newUser.Id == userId)//מנהל לא יכול ליצור את עצמו
        {
            return Unauthorized("Unauthorized: You don't have permission to perform this action.");
        }
        iUserService.Create(newUser);
        return CreatedAtAction(nameof(Create), new { id = newUser.Id }, newUser);
    }
    //מעדכן אוביקט מהרשימה
    [HttpPut("{id}")]
    [Authorize]
    public ActionResult Update(int id,  User newUser)
    {
        if (id != newUser.Id)
            return BadRequest("Id mismatch");
        (string type,int userId)=iAuthorizationService.GetUserClaims(User);
        if (iAuthorizationService.IsAccessDenied(id,type,userId)|| type.Equals("User")&&newUser.Type.Equals("Admin"))
        {
            return Unauthorized("Unauthorized: You don't have permission to perform this action.");
        }
        User? oldUser = iUserService.GetUserById(id);
        if (oldUser == null)
            return BadRequest("Invalid id");
        iUserService.Update(id, newUser);
        return NoContent();
    }
    //ID-פונקציה למחיקת אוביקט לפי 
    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    public ActionResult Delete(int id)
    {
         (string type,int userId)=iAuthorizationService.GetUserClaims(User);
        //האם מנהל יכול למחוק את עצמו
        //string jwtEncoded = Request.Headers.Authorization;
        User? userForDelete = iUserService.GetUserById(id);
        if (userForDelete == null)
            return BadRequest("Invalid id");
        iUserService.Delete(id,type,userId);
        return NoContent();
    }
[AllowAnonymous]
[HttpPost("login")]
public ActionResult Login([FromBody] User user)
{
 User? existUser =iUserService.GetExistUser(user);
    if (existUser == null)
      return  Unauthorized("Unauthorized: You don't have permission to perform this action.");
     string? generatedToken=iUserService.Login(existUser);

    if (string.IsNullOrEmpty(generatedToken ))
    {
        return StatusCode(500, "Error generating token");
    } 
    return Ok(new { Name = existUser.Name, token = generatedToken });
}
}
// [HttpPost("google")]
//         public async Task<IActionResult> GoogleLogin([FromBody] TokenRequest request)
//         {
//             try
//             {
//                 // בדיקת ה-ID Token מול Google
//                 var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);

//                 // payload מכיל את המידע על המשתמש
//                 var userId = payload.Subject;  // מזהה המשתמש בגוגל
//                 var email = payload.Email;     // אימייל של המשתמש
//                 var name = payload.Name;       // שם המשתמש
//                 var picture = payload.Picture; // תמונה (אם יש)

//                 // כאן תוכל לשמור את המידע הזה במאגר נתונים או לבצע פעולה אחרת

//                 return Ok(new { message = "Login successful", userId, email, name, picture });
//             }
//             catch (InvalidJwtException ex)
//             {
//                 return BadRequest(new { message = "Invalid token", error = ex.Message });
//             }
//         }
//     }

//     public class TokenRequest
//     {
//         public string Token { get; set; }
//     }



