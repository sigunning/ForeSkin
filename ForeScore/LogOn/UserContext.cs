namespace ForeScore.LogOn
{
    public class UserContext
    {
        public string Name { get; internal set; }
        public string UserIdentifier { get; internal set; }
        public bool IsLoggedOn { get; internal set; }
        public bool IsNewUser { get; internal set; }
        public string GivenName { get; internal set; }
        public string DisplayName { get; internal set; }
        public string FamilyName { get; internal set; }
        public string Province { get; internal set; }
        public string PostalCode { get; internal set; }
        public string Country { get; internal set; }
        public string EmailAddress { get; internal set; }
        public string JobTitle { get; internal set; }
        public string StreetAddress { get; internal set; }
        public string City { get; internal set; }
        public string AccessToken { get; internal set; }
        //Extra properties for app
        public string PlayerId { get; internal set; }
        public bool AdminYN { get; internal set; }
    }
}