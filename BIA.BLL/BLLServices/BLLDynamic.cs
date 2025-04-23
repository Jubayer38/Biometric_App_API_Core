using BIA.DAL.Repositories;
using BIA.Entity.DB_Model;
using BIA.Entity.ResponseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLDynamic
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLDynamic(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<List<UserType>> GetUserTypeDropdownValu()
        {

            List<UserType> userTypes = new List<UserType>();
            try
            {
                var dataRow = await _dataManager.GetUserTypeDropdownValu();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                { 
                    UserType usertype = new UserType();
                    usertype.UserTypeId = Convert.ToInt32(dataRow.Rows[i]["USERTYPE_ID"] == DBNull.Value ? null : dataRow.Rows[i]["USERTYPE_ID"]);
                    usertype.UserTypeName = Convert.ToString(dataRow.Rows[i]["USERTYPE_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["USERTYPE_NAME"]);
                    userTypes.Add(usertype);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return userTypes;
        }
        public async Task<List<ContentType>> GetContentTypeDropdownValue()
        {
            List<ContentType> contents = new List<ContentType>();
            try
            {
                var dataRow = await _dataManager.GetContentTypeDropdownValue(); 
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {
                    ContentType content = new ContentType();
                    content.contentTypeId = Convert.ToInt32(dataRow.Rows[i]["CONTENTTYPE_ID"] == DBNull.Value ? null : dataRow.Rows[i]["CONTENTTYPE_ID"]);
                    content.contentTypeName = Convert.ToString(dataRow.Rows[i]["CONTENTTYPE_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["CONTENTTYPE_NAME"]);
                    content.UserTypeId = Convert.ToInt32(dataRow.Rows[i]["USERTYPE_ID"] == DBNull.Value ? null : dataRow.Rows[i]["USERTYPE_ID"]);
                    contents.Add(content);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return contents;
        }
        public async Task<List<ContentUrl>> GetContentURL()
        {
            List<ContentUrl> urls = new List<ContentUrl>();
            try
            {
                var dataRow = await _dataManager.GetContentURL();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {

                    ContentUrl url = new ContentUrl();
                    url.urlId = Convert.ToInt32(dataRow.Rows[i]["CONTENTURL_ID"] == DBNull.Value ? null : dataRow.Rows[i]["CONTENTURL_ID"]);
                    url.url = Convert.ToString(dataRow.Rows[i]["CONTENTURLE_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["CONTENTURLE_NAME"]);
                    url.userTypeId = Convert.ToInt32(dataRow.Rows[i]["USERTYPE_ID"] == DBNull.Value ? null : dataRow.Rows[i]["USERTYPE_ID"]);
                    urls.Add(url);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return urls;
        }


    }
}
