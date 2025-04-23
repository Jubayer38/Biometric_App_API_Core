namespace BIA.Entity.ResponseEntity
{
    public class LoginUserInfoResponse
    {
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string role_id { get; set; }
        public string role_name { get; set; }
        public int? is_role_active { get; set; }
        public int? channel_id { get; set; }
        public string channel_name { get; set; }
        public int? is_activedirectory_user { get; set; }
        public string role_access { get; set; }
        public string distributor_code { get; set; }
        public int inventory_id { get; set; }
        public string center_code { get; set; }
        public string itopUpNumber { get; set; }
        public int is_default_Password { get; set; }
        public string ExpiredDate { get; set; }
        
    }

    public class LoginUserInfoResponseRev
    {
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string role_id { get; set; }
        public string role_name { get; set; }
        public int is_role_active { get; set; }
        public int channel_id { get; set; }
        public string channel_name { get; set; }
        public int is_activedirectory_user { get; set; }
        public string role_access { get; set; }
        public string distributor_code { get; set; }
        public int inventory_id { get; set; }
        public string center_code { get; set; }
        public string itopUpNumber { get; set; }
        public int is_default_Password { get; set; }
        public string ExpiredDate { get; set; }
        public string message { get; set; }
        public string designation { get; set; }
        public int isValidUser { get; set; }

    }

}
