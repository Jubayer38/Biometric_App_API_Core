using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class HelpButtonRespModel
    {
        public bool isError { get; set; }
        public string message { get; set; } 
        public List<UserType> data { get; set; }

    }

    public class UserType
    {
        public int UserTypeId { get; set; }
        public string UserTypeName { get; set; }
        public IEnumerable<ContentType> contentTypes { get; set; }
    }


    public class ContentType
    {
        public int contentTypeId { get; set; }
        public string contentTypeName { get; set; }
        public int UserTypeId { get; set; }
        public IEnumerable<ContentUrl> contentUrl { get; set; }
    }    

    public class ContentUrl
    {
        public int urlId { get; set; }
        public string url { get; set; }
        public int userTypeId { get; set; }
    }
     
}
 