using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLRAReqModelValidation
    {
        StringBuilder stringBuilder = new StringBuilder();
        private bool result = false;

        public bool ValidateOrderReq(RAOrderRequest order)
        {
            if (order == null)
                throw new ArgumentNullException("Request object can not be null.");

            #region Common parameter validation
            if (order.bi_token_number != null)
                throw new ArgumentNullException("'bi_token_number'can not be null.");

            if (String.IsNullOrEmpty(order.purpose_number))
                throw new ArgumentNullException("'purpose_number'can not be null.");

            if (!order.purpose_number.Any(Char.IsDigit))
                throw new ArgumentNullException("'purpose_number' should only contains numeric digits.");

            if (String.IsNullOrEmpty(order.msisdn))
                throw new ArgumentNullException("'msisdn'can not be null.");

            if (String.IsNullOrEmpty(order.retailer_id))
                throw new ArgumentNullException("'retailer_id'can not be null.");

            if (String.IsNullOrEmpty(order.channel_name))
                throw new ArgumentNullException("'channel_name'can not be null.");

            //if (String.IsNullOrEmpty(order.right_id))
            //    throw new ArgumentNullException("'right_id'can not be null.");

            if (String.IsNullOrEmpty(order.dest_nid))
                throw new ArgumentNullException("'dest_nid'can not be null.");

            if (String.IsNullOrEmpty(order.dest_dob))
                throw new ArgumentNullException("'dest_dob'can not be null.");


            if (order.dest_left_thumb_score == null)
                throw new ArgumentNullException("'dest_left_thumb_score'can not be null.");

            if (order.dest_left_index_score == null)
                throw new ArgumentNullException("'dest_left_index_score'can not be null.");

            if (String.IsNullOrEmpty(order.dest_left_index))
                throw new ArgumentNullException("'dest_left_index'can not be null.");

            if (order.dest_right_thumb_score == null)
                throw new ArgumentNullException("'dest_right_thumb_score'can not be null.");

            if (String.IsNullOrEmpty(order.dest_right_thumb))
                throw new ArgumentNullException("'dest_right_thumb'can not be null.");

            if (order.dest_right_index_score == null)
                throw new ArgumentNullException("'dest_right_index_score'can not be null.");

            if (String.IsNullOrEmpty(order.dest_right_index))
                throw new ArgumentNullException("'dest_right_index'can not be null.");

            #endregion

            #region purpose wise validation
            switch (Convert.ToInt16(order.purpose_number))
            {
                case (int)EnumPurposeNumber.NewRegistration:
                    break;

                case (int)EnumPurposeNumber.SIMReplacement:
                    break;

                case (int)EnumPurposeNumber.MNPRegistration:
                    break;

                case (int)EnumPurposeNumber.SIMTransfer:

                    #region Source customer subscription info 

                    if (String.IsNullOrEmpty(order.old_sim_number))
                        throw new ArgumentNullException("'old_sim_number' can not be null.");


                    if (order.dbss_subscription_id == null)
                        throw new ArgumentNullException("'dbss_subscription_id' can not be null.");

                    #endregion

                    #region Source customer info

                    if (String.IsNullOrEmpty(order.src_owner_customer_id))
                        throw new ArgumentNullException("'src_owner_customer_id' can not be null.");

                    if (String.IsNullOrEmpty(order.src_user_customer_id))
                        throw new ArgumentNullException("'src_user_customer_id' can not be null.");

                    if (String.IsNullOrEmpty(order.src_payer_customer_id))
                        throw new ArgumentNullException("'src_payer_customer_id' can not be null.");

                    #endregion

                    #region Source customer bio info

                    if (String.IsNullOrEmpty(order.src_nid))
                        throw new ArgumentNullException("'src_nid' can not be null.");

                    if (String.IsNullOrEmpty(order.src_dob))
                        throw new ArgumentNullException("'src_dob' can not be null.");


                    if (order.src_left_thumb_score == null)
                        throw new ArgumentNullException("'src_left_thumb_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_left_thumb))
                        throw new ArgumentNullException("'src_left_thumb' can not be null.");


                    if (order.src_left_index_score == null)
                        throw new ArgumentNullException("'src_left_index_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_left_index))
                        throw new ArgumentNullException("'src_left_index' can not be null.");

                    if (order.src_right_thumb_score == null)
                        throw new ArgumentNullException("'src_right_thumb_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_right_thumb))
                        throw new ArgumentNullException("'src_right_thumb' can not be null.");


                    if (order.src_right_index_score == null)
                        throw new ArgumentNullException("'src_right_index_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_right_index))
                        throw new ArgumentNullException("'src_right_index' can not be null.");



                    #endregion

                    #region Dest customer SAF info

                    if (String.IsNullOrEmpty(order.customer_name))
                        throw new ArgumentNullException("'customer_name' can not be null.");

                    if (String.IsNullOrEmpty(order.gender))
                        throw new ArgumentNullException("'gender' can not be null.");

                    if (order.division_id == null)
                        throw new ArgumentNullException("'division_id' can not be null.");

                    if (String.IsNullOrEmpty(order.division_name))
                        throw new ArgumentNullException("'division_name' can not be null.");

                    if (order.district_id == null)
                        throw new ArgumentNullException("'district_id' can not be null.");

                    if (String.IsNullOrEmpty(order.district_name))
                        throw new ArgumentNullException("'division_name' can not be null.");

                    if (order.thana_id == null)
                        throw new ArgumentNullException("'thana_id' can not be null.");

                    if (String.IsNullOrEmpty(order.thana_name))
                        throw new ArgumentNullException("'thana_name' can not be null.");

                    if (String.IsNullOrEmpty(order.village))
                        throw new ArgumentNullException("'village' can not be null.");

                    #endregion

                    break;

                default:
                    throw new ArgumentNullException("Invalid 'purpose_number'.");

            }
            #endregion
            return result = true;
        }

        public bool ValidateOrderReqV2(RAOrderRequestV2 order)
        {
            if (order == null)
                throw new ArgumentNullException("Request object can not be null.");

            #region Common parameter validation
            //if (order.bi_token_number != null)
            //    throw new ArgumentNullException("'bi_token_number'can not be null.");

            if (String.IsNullOrEmpty(order.purpose_number))
                throw new ArgumentNullException("'purpose_number'can not be null.");

            if (!order.purpose_number.Any(Char.IsDigit))
                throw new ArgumentNullException("'purpose_number' should only contains numeric digits.");

            if (String.IsNullOrEmpty(order.msisdn))
                throw new ArgumentNullException("'msisdn'can not be null.");

            if (String.IsNullOrEmpty(order.retailer_id))
                throw new ArgumentNullException("'retailer_id'can not be null.");

            if (String.IsNullOrEmpty(order.channel_name))
                throw new ArgumentNullException("'channel_name'can not be null.");

            //if (String.IsNullOrEmpty(order.right_id))
            //    throw new ArgumentNullException("'right_id'can not be null.");

            if (String.IsNullOrEmpty(order.dest_nid))
                throw new ArgumentNullException("'dest_nid'can not be null.");

            if (String.IsNullOrEmpty(order.dest_dob))
                throw new ArgumentNullException("'dest_dob'can not be null.");


            if (order.dest_left_thumb_score == null)
                throw new ArgumentNullException("'dest_left_thumb_score'can not be null.");

            if (order.dest_left_index_score == null)
                throw new ArgumentNullException("'dest_left_index_score'can not be null.");

            if (String.IsNullOrEmpty(order.dest_left_index))
                throw new ArgumentNullException("'dest_left_index'can not be null.");

            if (order.dest_right_thumb_score == null)
                throw new ArgumentNullException("'dest_right_thumb_score'can not be null.");

            if (String.IsNullOrEmpty(order.dest_right_thumb))
                throw new ArgumentNullException("'dest_right_thumb'can not be null.");

            if (order.dest_right_index_score == null)
                throw new ArgumentNullException("'dest_right_index_score'can not be null.");

            if (String.IsNullOrEmpty(order.dest_right_index))
                throw new ArgumentNullException("'dest_right_index'can not be null.");

            #endregion

            #region purpose wise validation
            switch (Convert.ToInt16(order.purpose_number))
            {
                case (int)EnumPurposeNumber.NewRegistration:
                    break;

                case (int)EnumPurposeNumber.SIMReplacement:
                    break;

                case (int)EnumPurposeNumber.MNPRegistration:
                    break;

                case (int)EnumPurposeNumber.SIMTransfer:

                    #region Source customer subscription info 

                    if (String.IsNullOrEmpty(order.old_sim_number))
                        throw new ArgumentNullException("'old_sim_number' can not be null.");


                    if (order.dbss_subscription_id == null)
                        throw new ArgumentNullException("'dbss_subscription_id' can not be null.");

                    #endregion

                    #region Source customer info

                    if (String.IsNullOrEmpty(order.src_owner_customer_id))
                        throw new ArgumentNullException("'src_owner_customer_id' can not be null.");

                    if (String.IsNullOrEmpty(order.src_user_customer_id))
                        throw new ArgumentNullException("'src_user_customer_id' can not be null.");

                    if (String.IsNullOrEmpty(order.src_payer_customer_id))
                        throw new ArgumentNullException("'src_payer_customer_id' can not be null.");

                    #endregion

                    #region Source customer bio info

                    if (String.IsNullOrEmpty(order.src_nid))
                        throw new ArgumentNullException("'src_nid' can not be null.");

                    if (String.IsNullOrEmpty(order.src_dob))
                        throw new ArgumentNullException("'src_dob' can not be null.");


                    if (order.src_left_thumb_score == null)
                        throw new ArgumentNullException("'src_left_thumb_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_left_thumb))
                        throw new ArgumentNullException("'src_left_thumb' can not be null.");


                    if (order.src_left_index_score == null)
                        throw new ArgumentNullException("'src_left_index_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_left_index))
                        throw new ArgumentNullException("'src_left_index' can not be null.");

                    if (order.src_right_thumb_score == null)
                        throw new ArgumentNullException("'src_right_thumb_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_right_thumb))
                        throw new ArgumentNullException("'src_right_thumb' can not be null.");


                    if (order.src_right_index_score == null)
                        throw new ArgumentNullException("'src_right_index_score' can not be null.");


                    if (String.IsNullOrEmpty(order.src_right_index))
                        throw new ArgumentNullException("'src_right_index' can not be null.");



                    #endregion

                    #region Dest customer SAF info

                    if (String.IsNullOrEmpty(order.customer_name))
                        throw new ArgumentNullException("'customer_name' can not be null.");

                    if (String.IsNullOrEmpty(order.gender))
                        throw new ArgumentNullException("'gender' can not be null.");

                    if (order.division_id == null)
                        throw new ArgumentNullException("'division_id' can not be null.");

                    if (String.IsNullOrEmpty(order.division_name))
                        throw new ArgumentNullException("'division_name' can not be null.");

                    if (order.district_id == null)
                        throw new ArgumentNullException("'district_id' can not be null.");

                    if (String.IsNullOrEmpty(order.district_name))
                        throw new ArgumentNullException("'division_name' can not be null.");

                    if (order.thana_id == null)
                        throw new ArgumentNullException("'thana_id' can not be null.");

                    if (String.IsNullOrEmpty(order.thana_name))
                        throw new ArgumentNullException("'thana_name' can not be null.");

                    if (String.IsNullOrEmpty(order.village))
                        throw new ArgumentNullException("'village' can not be null.");

                    #endregion

                    break;

                default:
                    throw new ArgumentNullException("Invalid 'purpose_number'.");

            }
            #endregion
            return result = true;
        }
    }
}
