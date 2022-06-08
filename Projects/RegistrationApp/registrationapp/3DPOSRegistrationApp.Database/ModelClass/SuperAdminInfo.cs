namespace _3DPOSRegistrationApp.Database.ModelClass
{
    public class SuperAdminInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
        public int UserStatus { get; set; }
        public string Password { get; set; }
    }
}
