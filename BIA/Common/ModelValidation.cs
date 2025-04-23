using BIA.Entity.ResponseEntity;

namespace BIA.Common
{
    public class ModelValidation
    {
        public ValidateOrderResponse OrderSubmitModelValidation(ValidationPropertiesModel model)
        {
            ValidateOrderResponse resp = new ValidateOrderResponse();
            try
            {
                resp.result = false;
                if (String.IsNullOrEmpty(model.purpose_number) || string.IsNullOrEmpty(model.msisdn) || string.IsNullOrEmpty(model.customer_name) || string.IsNullOrEmpty(model.gender) || model.division_id == 0 || model.division_id == null || model.district_id == 0 || model.district_id == null || model.thana_id == 0 || model.thana_id == null || string.IsNullOrEmpty(model.village))
                {
                    //if (String.IsNullOrEmpty(model.purpose_number))
                    //{
                    //    resp.message = "'Purpose_number' is required.";

                    //}
                    if (String.IsNullOrEmpty(model.msisdn))
                    {

                        resp.message = "Technical error!!! Customer data not captured, Please resubmit the request.";

                    }
                    else if (String.IsNullOrEmpty(model.customer_name))
                    {

                        resp.message = "Technical error!!! Customer data not captured, Please resubmit the request.";

                    }
                    else if (String.IsNullOrEmpty(model.gender))
                    {

                        resp.message = "Technical error!!! Customer data not captured, Please resubmit the request.";

                    }
                    else if (model.division_id == 0 || model.division_id == null)
                    {
                        resp.message = "Technical error!!! Customer data not captured, Please resubmit the request.";

                    }
                    else if (model.district_id == 0 || model.district_id == null)
                    {

                        resp.message = "Technical error!!! Customer data not captured, Please resubmit the request.";

                    }
                    else if (model.thana_id == 0 || model.thana_id == null)
                    {

                        resp.message = "Technical error!!! Customer data not captured, Please resubmit the request.";

                    }
                    else if (String.IsNullOrEmpty(model.village))
                    {

                        resp.message = "Technical error!!! Customer data not captured, Please resubmit the request.";

                    }
                }
                else
                {
                    resp.result = true;
                    resp.message = "Validation successful!";
                }

                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class ValidationPropertiesModel
        {
            public string purpose_number { get; set; }
            public string msisdn { get; set; }
            public string customer_name { get; set; }
            public string gender { get; set; }
            public int? division_id { get; set; }
            public int? district_id { get; set; }
            public int? thana_id { get; set; }
            public string village { get; set; }



        }


        public ValidateOrderResponse OrderReSubmitModelValidation(ValidationPropertiesResubmitModel model)
        {
            ValidateOrderResponse resp = new ValidateOrderResponse();
            try
            {
                resp.result = false;

                if (String.IsNullOrEmpty(model.bi_token_number))
                {

                    resp.message = "Technical error. RA Token not found!";

                }
                else
                {
                    resp.result = true;
                    resp.message = "Validation successful!";
                }

                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class ValidationPropertiesResubmitModel
        {
            public string bi_token_number { get; set; }

        }


    }
}
