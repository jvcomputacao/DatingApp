using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class AppUser
{
    public int Id { get; set; }

    public string Username { get; set; }
    public byte[] PassowrdHash { get; set; }

    public byte[] PassowrdSalt { get; set; }
}
